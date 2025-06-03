module DataUtils

open System
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



type consumoAtipico = {
    fecha: DateTime
    consumo: float
    zScore: float
    tipo: string  // "Pico" o "Caída"
}


// Definición del tipo CSV para cargar el archivo
[<Literal>]
let path = __SOURCE_DIRECTORY__ + @"\ConsumosAtipicos01.csv"

type CSV = CsvProvider<path, Separators=";", Culture="es-AR",  HasHeaders=true>

let cargarDatos (pathX:string) =
    CSV.Load(pathX).Rows
    |> Seq.map (fun row -> {diaGas=row.``DiaGas``; consumo=float row.GJ})
    |> Seq.toList



let graficar (datos:consumoDiario list) (cambios: result list) (titulo:string) =
    let fechas, consumos = datos |> List.map(fun x -> x.diaGas, x.consumo) |> List.unzip 
    
    let avgConsumos = consumos |> List.average

    let fechasCambios, limites = cambios |> List.map(fun x -> x.diaGas, x.limite) |> List.unzip
    let serie = Chart.Line(fechas, consumos, Name="Consumo Diario", LineWidth=0.5)

    let puntosCambio =
        Chart.Point(fechasCambios, limites, Name="Cambio Detectado")
        |> Chart.withMarker (Marker.init(Size=2, Color= Color.fromKeyword Red))


    Chart.combine [serie; puntosCambio]
    |> Chart.withTitle (titulo, TitleFont = Font.init(Size=10))
    |> Chart.withXAxisStyle("Fecha")
    |> Chart.withYAxisStyle("Consumo [GJ]")
    |> Chart.show


let graficarWin (datos:consumoDiario list) (cambios: result list) (winMin: float list) (winMax:float list) (titulo:string) =
    let fechas, consumos = datos |> List.map(fun x -> x.diaGas, x.consumo) |> List.unzip 
    
    let avgConsumos = consumos |> List.average

    let fechasCambios, limites = cambios |> List.map(fun x -> x.diaGas, x.limite) |> List.unzip
    let serie = Chart.Line(fechas, consumos, Name="Consumo Diario", LineWidth=0.5)

    let winMinSerie = Chart.Line(fechas, winMin, Name="Mínimo", LineWidth=0.5)
    
    let winMaxSerie = Chart.Line(fechas, winMax, Name="Máximo", LineWidth=0.5)


    let puntosCambio =
        Chart.Point(fechasCambios, limites, Name="Cambio Detectado")
        |> Chart.withMarker (Marker.init(Size=2, Color= Color.fromKeyword Red))


    Chart.combine [serie; puntosCambio; winMinSerie; winMaxSerie]
    |> Chart.withTitle (titulo, TitleFont = Font.init(Size=10))
    |> Chart.withXAxisStyle("Fecha")
    |> Chart.withYAxisStyle("Consumo [GJ]")
    |> Chart.show