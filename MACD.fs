module MACD

open DataUtils



// Calcular EMA genérica
let ema (period: int) (datos: float list) =
    let alpha = 2. / (float period + 1.)
    let rec loop xs prev acc =
        match xs with
        | [] -> List.rev acc
        | x::rest ->
            let e = alpha * x + (1. - alpha) * prev
            loop rest e (e::acc)
    match datos with
    | [] -> []
    | h::t -> loop t h [h]


let calcularMACD (datos: float list) =
    let ema12 = ema 12 datos
    let ema26 = ema 26 datos
    let macd = List.map2 (-) ema12.[ema12.Length - ema26.Length..] ema26
    let signal = ema 9 macd
    macd, signal




