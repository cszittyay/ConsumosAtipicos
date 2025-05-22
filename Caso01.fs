module Caso01

open System
open System.Numerics
open System.Globalization
open FSharp.Data
open Plotly.NET
open Plotly.NET.TraceObjects
open MathNet.Numerics
open MathNet.Numerics.IntegralTransforms


type consumoDiario = {
    diaGas: DateTime
    consumo: float
}



type ventana = {
    desde: DateTime
    hasta: DateTime
    min: float
    max: float
    avg: float
}


// === PARTE 1: Cargar CSV ===

// Definición del tipo CSV para cargar el archivo
[<Literal>]
let path = __SOURCE_DIRECTORY__ + @"\ConsumosAtipicos01.csv"

type CSV = CsvProvider<path, Separators=";", Culture="es-AR",  HasHeaders=true>

let cargarDatos (pathX:string) =
    CSV.Load(pathX).Rows
    |> Seq.map (fun row -> {diaGas=row.``DiaGas``; consumo=float row.GJ})
    |> Seq.toList

// === PARTE 2: Detección de Cambios ===

let detectarCambios (datos: consumoDiario list) (windowSize: int) (umbral: float) =
    let n = datos.Length
    [windowSize .. n - windowSize - 1]
    |> List.choose (fun i ->
        let prev = datos.[i - windowSize .. i - 1] |> List.map (fun x -> x.consumo)
        let next_ = datos.[i .. i + windowSize - 1] |> List.map (fun x -> x.consumo)
        let avgPrev = List.average prev
        let avgNext = List.average next_
        let delta = abs (avgNext - avgPrev)
        if delta > umbral then Some (datos.[i].diaGas, delta)
        else None
    )


// + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + 

// CUSUM unidireccional
//let detectarCambiosCUSUM (datos: consumoDiario list) (mediaEsperada: float) (umbral: float) =
//    let mutable sumaPos = 0.0
//    let mutable sumaNeg = 0.0

//    datos
//    |> List.mapi (fun i (fecha, consumo) ->
//        let delta = consumo - mediaEsperada
//        sumaPos <- max 0.0 (sumaPos + delta)
//        sumaNeg <- min 0.0 (sumaNeg + delta)

//        if sumaPos > umbral then
//            sumaPos <- 0.0
//            Some (fecha, "↑", consumo)
//        elif abs sumaNeg > umbral then
//            sumaNeg <- 0.0
//            Some (fecha, "↓", consumo)
//        else
//            None
//    )
//    |> List.choose id

//// Visualización con FSharp.Plotly
//let graficarCUSUM (datos: consumoDiario list) (cambios: consumoDiario list) =
//    let fechas, consumos = List.unzip datos

//    let fechasUp, valoresUp =
//        cambios
//        |> List.filter (fun (_, dir, _) -> dir = "↑")
//        |> List.map (fun (f, _, v) -> f, v)
//        |> List.unzip

//    let fechasDown, valoresDown =
//        cambios
//        |> List.filter (fun (_, dir, _) -> dir = "↓")
//        |> List.map (fun (f, _, v) -> f, v)
//        |> List.unzip

//    let serie =
//        Chart.Line(fechas, consumos, Name="Consumo Diario")

//    let puntosUp =
//        Chart.Point(fechasUp, valoresUp, Name="Cambio ↑")

//    let puntosDown =
//        Chart.Point(fechasDown, valoresDown, Name="Cambio ↓")

//    Chart.combine [serie; puntosUp; puntosDown]
//    |> Chart.withTitle "Consumo Diario con Detección CUSUM"
//    |> Chart.withXAxisStyle "Fecha"
//    |> Chart.withYAxisStyle "Consumo [GJ]"
//    |> Chart.show

//// === PARTE 3: Graficar ===

//let graficar datos cambios =
//    let fechas, consumos = List.unzip datos
//    let fechasCambios, _ = List.unzip cambios

//    let serie = 
//        Chart.Line(fechas, consumos, Name="Consumo Diario")

//    let puntosCambio =
//        Chart.Point(fechasCambios, List.replicate fechasCambios.Length 0.0, Name="Cambio Detectado")

//    Chart.combine [serie; puntosCambio]
//    |> Chart.withTitle "Consumo de Gas y Detección de Cambios"
//    |> Chart.withXAxisStyle("Fecha")
//    |> Chart.withYAxisStyle("Consumo [GJ]")
//    |> Chart.show



// + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + + 
// Ventana de tiempo para la detección de cambios


let windowSize = 7 // Tamaño de la ventana (en días)

let umbral = 30.0 // Umbral de cambio (en GJ)


let magnitudCambio (valor:float) (min:float) (max:float) tolerancia =
    if valor > max * (1.0 + tolerancia ) then (valor - max) / max * 100.0
    elif valor < min * (1.0 - tolerancia) && min > 0 then (valor - min) / min * 100.0
    else 0.


let magnitudCambioGJ (valor:float) (min:float) (max:float)  = 
    if valor > max then valor - max
    else  valor - min

let filtro consumo min max tolerancia =  consumo > max * (1.0 + tolerancia) || consumo < min * (1.0 - tolerancia) 
    
let getVentana  (datos: consumoDiario list) windowSize tolerancia retraso =
    let toleranciaPorc = tolerancia / 100.0
    let win =  datos |> List.windowed(windowSize) 
                   |> List.map(fun x -> 
                        {desde = x[0].diaGas;
                         hasta = x[windowSize-1].diaGas; 
                         min = x |> List.map(fun z -> z.consumo) |> List.min;
                         max = x |> List.map(fun z -> z.consumo) |> List.max;
                         avg = x |> List.averageBy(fun z -> z.consumo);
                         })
                   |> List.take (List.length datos - windowSize - retraso )
    let nuevaData = datos |> List.skip (windowSize + retraso)

    let cambios = win |> List.zip nuevaData 
                      |> List.filter(fun (x, y) -> filtro x.consumo y.min y.max toleranciaPorc)
                      // Determinar la magnitud del cambio
                      |> List.map(fun (x, y) -> x.diaGas, x.consumo, magnitudCambio x.consumo y.min y.max toleranciaPorc)
    cambios 




let getVentanaGJ  (datos: consumoDiario list) windowSize toleranciaGJ retraso =
    let win =  datos |> List.windowed(windowSize) 
                   |> List.map(fun x -> 
                        {desde = x[0].diaGas;
                         hasta = x[windowSize-1].diaGas; 
                         min = x |> List.map(fun z -> z.consumo) |> List.min;
                         max = x |> List.map(fun z -> z.consumo) |> List.max;
                         avg = x |> List.averageBy(fun z -> z.consumo);
                         })
                   |> List.take (List.length datos - windowSize - retraso )
    let nuevaData = datos |> List.skip (windowSize + retraso)

    let cambios = win |> List.zip nuevaData 
                      |> List.filter(fun (x, y) -> x.consumo > y.max + toleranciaGJ || x.consumo < y.min - toleranciaGJ)
                      // Determinar la magnitud del cambio
                      |> List.map(fun (x, y) -> x.diaGas, x.consumo, magnitudCambioGJ x.consumo y.min y.max)
    cambios 


// Detectar patrón de consumo semanal

let detectarFrecuencias (serie: float[]) =
    let cbuffer = serie |> Array.map(fun x -> Complex(x, 0.0))
    Fourier.Forward(cbuffer, FourierOptions.Default)
    cbuffer |> Array.map(fun x -> x.Magnitude) |> Array.toList |> List.skip 1 |> List.take 80 |> List.map(fun x -> x / 1000.0) |> List.iteri(fun i x -> printfn "%d\t%f" i x)


/// Calcula la autocorrelación de una serie con un lag específico
let autocorrelacion (serie: float list) (lag: int) =
    let n = serie.Length - lag
    let baseSerie = serie |> List.take n
    let lagSerie = serie |> List.skip lag |> List.take n
    let media xs = List.average xs
    let desv xs =
        let m = media xs
        sqrt (List.averageBy (fun x -> (x - m) ** 2.0) xs)
    let covarianza =
        List.zip baseSerie lagSerie
        |> List.averageBy (fun (x, y) -> (x - media baseSerie) * (y - media lagSerie))
    covarianza / (desv baseSerie * desv lagSerie)



let graficar (datos:consumoDiario list) (fechasCambios: DateTime list) =
    let fechas, consumos = datos |> List.map(fun x -> x.diaGas, x.consumo) |> List.unzip 
    
    let avgConsumos = consumos |> List.average

    let serie = 
        Chart.Line(fechas, consumos, Name="Consumo Diario", LineWidth=1.0)

    let puntosCambio =
        Chart.Point(fechasCambios, List.replicate fechasCambios.Length avgConsumos, Name="Cambio Detectado")
        |> Chart.withMarker (Marker.init(Size=2, Color= Color.fromKeyword Red))

    Chart.combine [serie; puntosCambio]
    |> Chart.withTitle "Consumo de Gas y Detección de Cambios"
    |> Chart.withXAxisStyle("Fecha")
    |> Chart.withYAxisStyle("Consumo [GJ]")
    |> Chart.show
