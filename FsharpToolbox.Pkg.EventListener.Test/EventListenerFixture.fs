namespace FsharpToolboxPkg.EventListener.Test

open System
open System.Threading.Tasks
open FsharpToolbox.Pkg.Communication.Core
open Microsoft.Extensions.DependencyInjection
open FsharpToolboxPkg.FpUtils
open FsharpToolboxPkg.Serialization.Json

module EventListenerFixture =

  type FakeScope() =
    interface IServiceScope with
      override _.ServiceProvider: IServiceProvider = FakeServiceProvider() :> IServiceProvider
      member this.Dispose() = ()

  and FakeServiceProvider() =
    interface IServiceProvider with
      override _.GetService theType: obj =
        match theType with
        | t when t = typedefof<IServiceScopeFactory> -> FakeServiceProvider() :> obj
        | _ -> () :> obj

    interface IServiceScopeFactory with
      override _.CreateScope () = new FakeScope() :> IServiceScope


  type FakeEventReceiver(receiverType: ReceiverType) =
    let mutable eventHandler: Option<Func<Event,Task<EventResult>>> = None
    interface IEventReceiver with
      member this.RegisterHandler(onEvent, _): unit =
        eventHandler <- Some onEvent
      member this.CloseAsync(): Task =
        Task.CompletedTask
      member this.ReceiverType(): ReceiverType =
        receiverType
      member this.ReceiverChannelName(): string =
        "channelName"

    member this.ReceiveEvent (event: Event): Task<EventResult> =
      match eventHandler with
      | Some handler -> handler.Invoke(event)
      | None -> failwith ("Tried to call event handler when it is None")

  let createEvent (name: string) (version: string) (payload: obj) =
    Event()
    |= fun e -> e.EventName <- name
    |= fun e -> e.Version <- version
    |= fun e -> e.Payload <- Serializer.jsonSerialize payload
