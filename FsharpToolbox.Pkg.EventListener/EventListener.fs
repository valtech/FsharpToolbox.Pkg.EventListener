namespace FsharpToolboxPkg.EventListener

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open System
open System.Threading
open System.Threading.Tasks

open FsharpToolbox.Pkg.Communication.Core
open FsharpToolboxPkg.FpUtils
open FsharpToolboxPkg.Logging
open FsharpToolboxPkg.Serialization.Json
open FsharpToolboxPkg.Serialization.Json.Serializer

type EventNameAndVersion = {
  name: string
  version: string
}

type EventProcessingFunc = IServiceProvider -> string -> Result<unit, string>

type EventListenerCallbacks = {
  onEvent: Map<EventNameAndVersion, EventProcessingFunc>
}

type EventListener
  (
    serviceProvider: IServiceScopeFactory,
    eventReceiver: IEventReceiver,
    eventCallbacks: EventListenerCallbacks
  ) =

  let onException e =
    L.Error(e, "Error from service bus")
    Task.CompletedTask

  let logErrorsAndDeadLetter error =
    L.Error(("Unable to process event. Error: {@Error}", error))
    DeadLetterResult(Message = sprintf "Error: %A" error)

  let runInScope handler (dto: 'DTO) =
    try
      use scope = serviceProvider.CreateScope()
      handler scope.ServiceProvider dto
    with
      | ex ->
        L.Error(ex, ("Could not process event: {message}", ex.Message))
        ex.Message |> Error

  let handleProcessResult res: EventResult =
    match res with
      | Ok _ -> upcast CompleteResult()
      | Error e -> upcast logErrorsAndDeadLetter e

  let runHandlerIfSupportedVersion targets (ev: Event) =
    targets
    |> Map.tryFind ({ name = ev.EventName; version = ev.Version }: EventNameAndVersion)
    |> function
       | Some f ->
         L.Info
           (("Starting processing of event {eventName} with version {version} and payload {payload}",
             ev.EventName,
             ev.Version,
             ev.Payload))
         ev.Payload
         |> runInScope f
         |> handleProcessResult
       | None ->
         upcast DeadLetterResult(Message = sprintf "Unknown event version %s for EventName: %s" ev.Version ev.EventName)

  let handleEventTypeNotSupported (event: Event): EventResult =
    match eventReceiver.ReceiverType() with
    | ReceiverType.Queue ->
      L.Error
        (("Failed to find a target for the event name {eventName} with version {version} containing payload {payload}",
          event.EventName,
          event.Version,
          event.Payload))
      upcast DeadLetterResult(
        Message = sprintf "Failed to find a target for the event name %s with version %s"
                    event.EventName
                    event.Version)
    | ReceiverType.Topic ->
      L.Info(("Did not find a registered target for event: {eventName} with version {version} ...skipping", event.EventName, event.Version))
      upcast CompleteResult()
    | _ -> upcast DeadLetterResult(Message = "Unknown ReceiverType. Only Queue/Topic are supported")

  let dispatchToCallback
    (targets: Map<EventNameAndVersion, EventProcessingFunc>)
    (ev: Event)
    : Task<EventResult> =
      targets
      |> Map.toList
      |> List.map (fun (k, _) -> k.name)
      |> Set.ofList
      |> Set.exists (fun k -> k.Equals(ev.EventName))
      |> function
         | true ->
           runHandlerIfSupportedVersion targets ev
         | false ->
           handleEventTypeNotSupported ev
      |> Task.FromResult

  static member private extractDto (payload: string): Result<'DTO, string> =
    try
      Serializer.jsonDeserialize<'DTO> DeserializeSettings.AllowAll payload
      |= fun _ -> L.Info(("Successfully converted event to DTO: {payload}", payload))
      |> Ok
    with
    | ex ->
      let errMsg =
        sprintf
          "Error when converting to type %s from payload: %s"
          typedefof<'DTO>.FullName
          payload
      L.Error(ex, errMsg)
      errMsg |> Error

  static member createEventProcessor<'DTO> handler: EventProcessingFunc =
    fun (scope: IServiceProvider) json ->
      json
      |> EventListener.extractDto<'DTO>
      >>= (handler scope)

  member _.startAsync () =
    let constructedHandler = dispatchToCallback eventCallbacks.onEvent
    eventReceiver.RegisterHandler(Func<Event, Task<EventResult>> constructedHandler, Func<exn, Task> onException)
    (sprintf "Starting event listener. %s name: %s"
       (eventReceiver.ReceiverType().ToString())
       (eventReceiver.ReceiverChannelName()))
    |> L.Info
    Task.CompletedTask

  member _.stopAsync () = eventReceiver.CloseAsync()

  interface IHostedService with
    member this.StartAsync(_: CancellationToken) = this.startAsync ()
    member this.StopAsync(_: CancellationToken) = this.stopAsync ()

