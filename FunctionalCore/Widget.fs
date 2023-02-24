namespace FunctionalCore

open System

module Widget =

    type Command =
        | AddWidget of name: string * initialStock: uint
        | PurchaseNewStock of widgetCount: uint
        | SellWidgets of widgetCount: uint
        | ReserveWidgets of requestedBy: string * reservationId: string * quantity: uint
        | FulfillReservation of reservationId: string
        | RemoveWidgetFromInventory

    type Event =
        | WidgetAdded of widgetId: Guid * name: string * initialStock: uint
        | WidgetsSold of widgetId: Guid * quantity: uint * remainingStock: uint
        | WidgetsNotSold of widgetId: Guid * reason: string
        | WidgetStockReplenished of widgetId: Guid * quantity: uint * currentStock: uint
        | WidgetRemovedFromInventory of widgetId: Guid
        | WidgetNotRemovedFromInventory of widgetId: Guid * reason: string
        | ReservationAdded of widgetId: Guid * reservedFor: string * reservationId: string * quantity: uint
        | ReservationNotAdded of widgetId: Guid * reason: string
        | ReservationFulfilled of widgetId: Guid * reservationId: string * quantity: uint * remainingStock: uint
        | ReservationNotFulfilled of widgetId: Guid * reservationId: string * reason: string

    type Reservation = { reservedFor: string; quantity: uint }

    type ActiveState =
        { name: string
          stock: uint
          reservations: Map<string, Reservation> }

    type State =
        | New of widgetId: Guid
        | Active of widgetId: Guid * state: ActiveState
        | Archived of widgetId: Guid * name: string

    let decide command state =
        match (state, command) with
        | New widgetId, AddWidget (name, quantity) -> [ WidgetAdded(widgetId, name, quantity) ]
        | Archived _, _ -> []
        | Active (widgetId, { stock = s }), SellWidgets count when count <= s ->
            [ WidgetsSold(widgetId, count, s - count) ]
        | Active (widgetId, _), SellWidgets _ -> [ WidgetsNotSold(widgetId, "Insufficient stock") ]
        | Active (widgetId, { stock = s }), PurchaseNewStock quantity ->
            [ WidgetStockReplenished(widgetId, quantity, s + quantity) ]
        | Active (widgetId, { stock = s; reservations = r }), RemoveWidgetFromInventory
            when s = 0u && r.Count = 0
            -> [ WidgetRemovedFromInventory widgetId ]
        | Active (widgetId, _), RemoveWidgetFromInventory ->
            [ WidgetNotRemovedFromInventory(widgetId, "stock or reservations are non-zero") ]
        | Active (widgetId, { reservations = r }), ReserveWidgets (reservedFor, reservationId, quantity)
            when r.ContainsKey(reservationId) = false
            ->
                [ ReservationAdded(widgetId, reservedFor, reservationId, quantity) ]
        | Active (widgetId, _), ReserveWidgets _ -> [ ReservationNotAdded(widgetId, "Reservation id exists") ]
        | Active (widgetId, { stock = s; reservations = r }), FulfillReservation reservationId when
            r.ContainsKey(reservationId) && r[reservationId].quantity <= s
            ->
                [ ReservationFulfilled(widgetId, reservationId, r[reservationId].quantity, s - r[reservationId].quantity) ]
        | Active (widgetId, _), FulfillReservation reservationId ->
            [ ReservationNotFulfilled(widgetId, reservationId, "Insufficient quantity or not found") ]
        | _ -> []

    let evolve state event =
        match (state, event) with
        | New widgetId, WidgetAdded (_, name, quantity) ->
            Active(
                widgetId,
                { name = name
                  stock = quantity
                  reservations = Map.empty }
            )
        | Active (widgetId, state), WidgetsSold (_, _, currentStock) ->
            Active(widgetId, { state with stock = currentStock })
        | Active (widgetId, state), WidgetStockReplenished (_, _, currentStock) ->
            Active(widgetId, { state with stock = currentStock })
        | Active (widgetId, state), ReservationAdded (_, reservedFor, reservationId, quantity) ->
            Active(
                widgetId,
                { state with
                    reservations =
                        state.reservations.Add(
                            reservationId,
                            { reservedFor = reservedFor
                              quantity = quantity }
                        ) }
            )
        | Active (widgetId, state), ReservationFulfilled (_, reservationId, _, remainingStock) ->
            Active(
                widgetId,
                { state with
                    stock = remainingStock
                    reservations = state.reservations.Remove(reservationId) }
            )
        | _ -> state
