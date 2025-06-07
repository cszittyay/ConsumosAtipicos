module ChatGPT_01

open System
open DataUtils

/// Calcula la media móvil de tamaño n
let movingAverage (n: int) (datos: consumoDiario list) : float list =
    datos
    |> List.map (fun x -> x.consumo)
    |> List.windowed n
    |> List.map List.average
    |> fun xs -> List.replicate (n - 1) nan @ xs

/// Calcula bandas superior e inferior con tolerancia
let calcularBandas (media: float list) (tolerancia: float) : (float list * float list) =
    let factor = tolerancia / 100.0
    let sup = media |> List.map (fun m -> m * (1.0 + factor))
    let inf = media |> List.map (fun m -> m * (1.0 - factor))
    sup, inf

/// Detecta valores atípicos por fuera de bandas
let detectarAtipicos (datos: consumoDiario list) (media: float list) (sup: float list) (inf: float list) =
    // let datosValidos = List.skip (media |> List.takeWhile Double.IsNaN |> List.length) datos
    zip4 datos media sup inf
    |> List.choose (fun (d, m, s, i) ->
        if d.consumo > s || d.consumo < i then Some(d) else None)

/// Ejemplo de uso
let detectarOutliers datos ventana tolerancia =
    let media = movingAverage ventana datos
    let (sup, inf) = calcularBandas media tolerancia
    let result = detectarAtipicos datos media sup inf |> List.map (fun x -> { diaGas = x.diaGas; consumo = x.consumo; limite = x.consumo })

    result, inf, sup
