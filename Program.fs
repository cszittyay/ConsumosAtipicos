open System
open Plotly.NET
open Caso01
open Pareto
open DataUtils
open Autocorrelacion
open AnalisisFrecuencial
open zscore
open MACD
open CumSum1
open ChatGPT_01



let pathBase = @"C:\Users\cszit\source\repos\f#\MX\ConsumosAtipicos\ConsumosAtipicos\"
let archivo =  $"ConsumosAtipicos02"
let pathArchivo =  pathBase + archivo + ".csv" // @"ConsumosAtipicos02.csv"
//    


// let datos = cargarDatos pathArchivo 

// datos |> List.map(fun x -> x.consumo) |> autocorrelacionMovil 7 |> List.map(fun x -> x * 100.0) |> List.iter (printfn "%f")





// let cambios = getVentanaGJ datos 10 100 2 //  |> Seq.map (fun (x, y, z) -> x) |> Seq.toList
//graficar datos cambios  





//let datos = cargarDatos pathArchivo |> List.map(fun x -> x.consumo)
//autocorrelacion datos 6 |> printfn "%f" 

[<EntryPoint>]
let main argv =
    //let avg = 40.0
    //let umbral = 20.0
    //let cambios = detectarCambiosCUSUM datos avg umbral
    //graficarCUSUM datos cambios

    // analisis de los argumentos de entrada
    
    let archivo = argv.[0]      // "s##"
    let ventana = argv.[1] |> int
    let umbral = argv.[2]|> float
    let retraso = argv.[3] |> int

    // PARETO

    //let datos = cargarDatos pathArchivo
    //let pareto = getPareto2 datos 0.1
    //cambiosPareto datos (fst pareto) (snd pareto)
    //printfn "%A" pareto


    // zona de atipicos 
    //let zscore = detectarAtipicosZS datos 21 2

    //zscore |> Seq.iter (printfn "%A")

    // graficarZScore datos "Detección de Atípicos con Z-Score"

    let pathBase = @"C:\Users\cszit\source\repos\f#\MX\ConsumosAtipicos\ConsumosAtipicos\"
    let archivo =  $"ConsumosAtipico{archivo}"
    let pathArchivo =  pathBase + archivo + ".csv" // @"ConsumosAtipicos02.csv"
     
    let titulo = $"Archivo: {archivo}\tVentana: {ventana}días,\tUmbral: {umbral},\tRetraso: {retraso}días"
    printfn "%s" titulo
    let datos = cargarDatos pathArchivo
    

    printfn $"Archivo:{archivo}\tVentana:{ventana}\tUmbral:{umbral}\tRetraso:{retraso}"

    let cambios,winMin, winMax = getVentana datos ventana umbral retraso 
                  
    graficarWin datos cambios winMin winMax titulo
    
    

    0
