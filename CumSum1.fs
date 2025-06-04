module CumSum1
open DataUtils
open Plotly
open Plotly.NET
open Plotly.NET.TraceObjects


/// Detecta cambios estructurales con CUSUM (desviación acumulada)
// k: umbral de desviación
// h: umbral de cambio
let detectarCUSUM (datos: consumoDiario list) (mediaEsperada: float) (k: float) (h: float) =
    let rec loop acumulado cambios (restantes: consumoDiario list) =
        match restantes with
        | [] -> List.rev cambios
        | x::xs ->
            let s' = max 0.0 (acumulado + x.consumo - mediaEsperada - k)
            let cambios' =
                if s' > h then
                    // Cambio detectado
                    { diaGas = x.diaGas; consumo = x.consumo } :: cambios
                else cambios
            loop s' cambios' xs
    loop 0.0 [] datos




let graficarCSUM  (datos:consumoDiario list) (cambios: result list)  (titulo:string) =
    let fechas, consumos = datos |> List.map(fun x -> x.diaGas, x.consumo) |> List.unzip 
    
    let avgConsumos = consumos |> List.average

    let fechasCambios, limites = cambios |> List.map(fun x -> x.diaGas, x.consumo) |> List.unzip
    let serie = Chart.Line(fechas, consumos, Name="Consumo Diario", LineWidth=0.5)

    
    let layout =
        let tmp = Layout()
        tmp?width <- 1200
        tmp?height <- 980    
        tmp

    let puntosCambio =
        Chart.Point(fechasCambios, limites, Name="Cambio Detectado")
        |> Chart.withMarker (Marker.init(Size=2, Color= Color.fromKeyword Red))
        |> Chart.withLayout(layout)
    

    Chart.combine [serie; puntosCambio]
    |> Chart.withTitle (titulo, TitleFont = Font.init(Size=10))
    |> Chart.withXAxisStyle("Fecha")
    |> Chart.withYAxisStyle("Consumo [GJ]")
    |> Chart.show