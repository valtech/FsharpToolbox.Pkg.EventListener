namespace FsharpToolboxPkg.EventListener.Test


open FsharpToolbox.Pkg.Communication.Core
open FsharpToolboxPkg.EventListener
open FsharpToolboxPkg.Serialization.Json.Serializer
open NUnit.Framework
open FsharpToolboxPkg.FpUtils

open FsharpToolboxPkg.Serialization.Json
open FsharpToolboxPkg.EventListener.Test.EventListenerFixture

module TopicEventListenerTest =

  type Currency =
    | SEK
    | USD

  type PaidAmount = {
    amount: int
    currency: Currency
  }


  [<Test>]
  let ``Supported EventName with supported Version and correct Payload returns CompleteResult`` () =
    // Arrange
    let eventCallbacks: EventListenerCallbacks = {
      onEvent =
      [{name = "OrderPaid"; version = "2"},
        fun _ (ev: string) ->
          try
            Serializer.jsonDeserialize<PaidAmount> DeserializeSettings.Default ev |> Ok
            >>= fun _ -> Ok ()
          with
            ex -> Error ex.Message
          ] |> Map.ofList
    }
    let evReceiver = FakeEventReceiver(ReceiverType.Topic)
    let eventReceiver =
      EventListener(FakeServiceProvider(), evReceiver, eventCallbacks)
    let event = createEvent "OrderPaid" "2" {amount = 29900; currency = Currency.SEK}

    // Act
    eventReceiver.startAsync() |> ignore
    let res =
      evReceiver.ReceiveEvent event
      |> Async.AwaitTask
      |> Async.RunSynchronously

    // Assert
    Assert.AreEqual(typedefof<CompleteResult>, res.GetType())

  [<Test>]
  let ``Supported EventName with supported Version and Payload in PascalCase returns CompleteResult`` () =
    // Arrange
    let eventCallbacks: EventListenerCallbacks = {
      onEvent =
      [{name = "OrderPaid"; version = "2"},
        fun _ (ev: string) ->
          try
            Serializer.jsonDeserialize<PaidAmount> DeserializeSettings.Default ev |> Ok
            >>= fun _ -> Ok ()
          with
            ex -> Error ex.Message
          ] |> Map.ofList
    }
    let evReceiver = FakeEventReceiver(ReceiverType.Topic)
    let eventReceiver =
      EventListener(FakeServiceProvider(), evReceiver, eventCallbacks)
    let event = createEvent "OrderPaid" "2" {|Amount = 29900; Currency = Currency.SEK|}

    // Act
    eventReceiver.startAsync() |> ignore
    let res =
      evReceiver.ReceiveEvent event
      |> Async.AwaitTask
      |> Async.RunSynchronously

    // Assert
    Assert.AreEqual(typedefof<CompleteResult>, res.GetType())

  [<Test>]
  let ``Supported EventName with supported Version and Payload in Upper-case returns CompleteResult`` () =
    // Arrange
    let eventCallbacks: EventListenerCallbacks = {
      onEvent =
      [{name = "OrderPaid"; version = "2"},
        fun _ (ev: string) ->
          try
            Serializer.jsonDeserialize<PaidAmount> DeserializeSettings.Default ev |> Ok
            >>= fun _ -> Ok ()
          with
            ex -> Error ex.Message
          ] |> Map.ofList
    }
    let evReceiver = FakeEventReceiver(ReceiverType.Topic)
    let eventReceiver =
      EventListener(FakeServiceProvider(), evReceiver, eventCallbacks)
    let event = createEvent "OrderPaid" "2" {|AMOUNT = 29900; CURRENCY = Currency.SEK|}

    // Act
    eventReceiver.startAsync() |> ignore
    let res =
      evReceiver.ReceiveEvent event
      |> Async.AwaitTask
      |> Async.RunSynchronously

    // Assert
    Assert.AreEqual(typedefof<CompleteResult>, res.GetType())

  [<Test>]
  let ``Supported EventName wrong Version and correct Payload returns DeadLetter`` () =
    // Arrange
    let eventCallbacks: EventListenerCallbacks = {
      onEvent =
      [{name = "OrderPaid"; version = "2"},
        fun _ (ev: string) ->
          try
            Serializer.jsonDeserialize<PaidAmount> DeserializeSettings.Default ev |> Ok
            >>= fun _ -> Ok ()
          with
            ex -> Error ex.Message
          ] |> Map.ofList
    }
    let evReceiver = FakeEventReceiver(ReceiverType.Topic)
    let eventReceiver =
      EventListener(FakeServiceProvider(), evReceiver, eventCallbacks)
    let event =
      createEvent
        "OrderPaid"
        "1" // Unknown event version
        {amount = 29900; currency = Currency.SEK}

    // Act
    eventReceiver.startAsync() |> ignore
    let res =
      evReceiver.ReceiveEvent event
      |> Async.AwaitTask
      |> Async.RunSynchronously

    // Assert
    Assert.AreEqual(typedefof<DeadLetterResult>, res.GetType())
    Assert.AreEqual("Unknown event version 1 for EventName: OrderPaid", (res :?> DeadLetterResult).Message)

  [<Test>]
  let ``Unsupported EventName with arbitrary Version and correct Payload returns CompleteResult`` () =
    // Arrange
    let eventCallbacks: EventListenerCallbacks = {
      onEvent =
      [{name = "OrderPaid"; version = "2"},
        fun _ (ev: string) ->
          try
            Serializer.jsonDeserialize<PaidAmount> DeserializeSettings.Default ev |> Ok
            >>= fun _ -> Ok ()
          with
            ex -> Error ex.Message
          ] |> Map.ofList
    }
    let evReceiver = FakeEventReceiver(ReceiverType.Topic)
    let eventReceiver =
      EventListener(FakeServiceProvider(), evReceiver, eventCallbacks)
    let event =
      createEvent
        "MoneyPaid" // Unknown event name
        "1"
        {amount = 29900; currency = Currency.SEK}

    // Act
    eventReceiver.startAsync() |> ignore
    let res =
      evReceiver.ReceiveEvent event
      |> Async.AwaitTask
      |> Async.RunSynchronously

    // Assert
    Assert.AreEqual(typedefof<CompleteResult>, res.GetType())

  [<Test>]
  let ``Supported EventName with supported Version and malformed Payload returns DeadLetter`` () =
    // Arrange
    let eventCallbacks: EventListenerCallbacks = {
      onEvent =
      [{name = "OrderPaid"; version = "2"},
        fun _ (ev: string) ->
          try
            Serializer.jsonDeserialize<PaidAmount> DeserializeSettings.Default ev |> Ok
            >>= fun _ -> Ok ()
          with
            ex -> Error ex.Message
          ] |> Map.ofList
    }
    let evReceiver = FakeEventReceiver(ReceiverType.Topic)
    let eventReceiver =
      EventListener(FakeServiceProvider(), evReceiver, eventCallbacks)
    let event =
      createEvent
        "OrderPaid"
        "1"
        {|aamount = 29900; currency = Currency.SEK|} // Wrong payload

    // Act
    eventReceiver.startAsync() |> ignore
    let res =
      evReceiver.ReceiveEvent event
      |> Async.AwaitTask
      |> Async.RunSynchronously

    // Assert
    Assert.AreEqual(typedefof<DeadLetterResult>, res.GetType())
