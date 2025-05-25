module Caso01

open System
open System.Numerics
open System.Globalization
open FSharp.Data
open Plotly.NET
open Plotly.NET.TraceObjects


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

type result = {
    diaGas: DateTime
    consumo: float
    limite: float
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
// Ventana de tiempo para la detección de cambios



let magnitudCambio (valor:float) (min:float) (max:float) tolerancia =
    if valor > max * (1.0 + tolerancia ) then (valor - max) / max * 100.0
    elif valor < min * (1.0 - tolerancia) && min > 0 then (valor - min) / min * 100.0
    else 0.


let magnitudCambioGJ (valor:float) (min:float) (max:float)  = 
    if valor > max then valor - max
    else  valor - min

let filtro consumo min max tolerancia =  consumo > max * (1.0 + tolerancia) || consumo < min * (1.0 - tolerancia) 

let getVentanaBase  (datos: consumoDiario list) windowSize retraso =
    let win =  datos |> List.windowed(windowSize) 
                   |> List.map(fun x -> 
                        {desde = x[0].diaGas;
                         hasta = x[windowSize-1].diaGas; 
                         min = x |> List.map(fun z -> z.consumo) |> List.min;
                         max = x |> List.map(fun z -> z.consumo) |> List.max;
                         avg = x |> List.averageBy(fun z -> z.consumo);
                         })
                   |> List.take (List.length datos - windowSize - retraso )
    win


let getVentanaBase2  (datos: consumoDiario list) windowSize retraso =
    let win =  datos |> List.windowed(windowSize) 
                   |> List.map(fun x -> 
                        let win2 = x |> List.take (windowSize - 1)
                        let consumoUD = x.[windowSize-1].consumo
                        let ajusteConsumo c1 c2 = c1 + (c2 - c1) / 4.0

                        let win3 =
                            {desde = x[0].diaGas;
                             hasta = x[windowSize-1].diaGas; 
                             min = win2 |> List.map(fun z -> z.consumo) |> List.min;
                             max = win2 |> List.map(fun z -> z.consumo) |> List.max;
                             avg = x |> List.averageBy(fun z -> z.consumo);
                             }
                        // ajustar el consumo de la ventana para que no se vea afectado por el nuevo consumo
                        if win3.max < consumoUD then {win3 with max = ajusteConsumo win3.max consumoUD} 
                        elif win3.min > consumoUD then {win3 with min = ajusteConsumo consumoUD win3.min } 
                        else win3)

                   |> List.take (List.length datos - windowSize - retraso )
    win
      

      
let getVentana  (datos: consumoDiario list) windowSize tolerancia retraso =
    let toleranciaPorc = tolerancia / 100.0
    let win =  getVentanaBase2 datos windowSize retraso

    let nuevaData = datos |> List.skip (windowSize + retraso)

    let cambios = win |> List.zip nuevaData 
                      |> List.filter(fun (x, y) -> filtro x.consumo y.min y.max toleranciaPorc)
                      // Determinar la magnitud del cambio
                      |> List.map(fun (x, y) -> 
                            let lim = if x.consumo > y.max * (1.+ toleranciaPorc) then y.max else y.min
                            {diaGas = x.diaGas; consumo = x.consumo; limite = lim})
    cambios


let getVentanaGJ  (datos: consumoDiario list) windowSize toleranciaGJ retraso =
    
    let win =  getVentanaBase2 datos windowSize retraso
    let nuevaData = datos |> List.skip (windowSize + retraso)

    let cambios = win |> List.zip nuevaData 
                      |> List.filter(fun (x, y) -> x.consumo > y.max + toleranciaGJ || x.consumo < y.min - toleranciaGJ)
                      // Determinar la magnitud del cambio
                      |> List.map(fun (x, y) -> 
                            let lim = if x.consumo > y.max+toleranciaGJ then y.max else y.min
                            {diaGas = x.diaGas; consumo = x.consumo; limite = lim})
    cambios


let graficar (datos:consumoDiario list) (cambios: result list) (titulo:string) =
    let fechas, consumos = datos |> List.map(fun x -> x.diaGas, x.consumo) |> List.unzip 
    
    let avgConsumos = consumos |> List.average

    let fechasCambios, limites = cambios |> List.map(fun x -> x.diaGas, x.limite) |> List.unzip
    let serie = 
        Chart.Line(fechas, consumos, Name="Consumo Diario", LineWidth=1.0)

    let puntosCambio =
        Chart.Point(fechasCambios, limites, Name="Cambio Detectado")
        |> Chart.withMarker (Marker.init(Size=2, Color= Color.fromKeyword Red))


    Chart.combine [serie; puntosCambio]
    |> Chart.withTitle (titulo, TitleFont = Font.init(Size=10))
    |> Chart.withXAxisStyle("Fecha")
    |> Chart.withYAxisStyle("Consumo [GJ]")
    |> Chart.show
