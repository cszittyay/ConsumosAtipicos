open System
open Plotly.NET
open Caso01
open Pareto
open DataUtils
open Autocorrelacion
open AnalisisFrecuencial
open zscore



let pathBase = @"C:\Users\cszit\source\repos\f#\MX\ConsumosAtipicos\ConsumosAtipicos\"
let archivo =  $"ConsumosAtipicos02"
let pathArchivo =  pathBase + archivo + ".csv" // @"ConsumosAtipicos02.csv"
//    


// let datos = cargarDatos pathArchivo 

// datos |> List.map(fun x -> x.consumo) |> autocorrelacionMovil 7 |> List.map(fun x -> x * 100.0) |> List.iter (printfn "%f")


// datos |> List.map(fun x -> x.consumo) |> List.toArray |> detectarFrecuencias



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
    
    let modo = argv.[0]
    let archivo = argv.[1]      // "s##"
    let ventana = argv.[2] |> int
    let umbral = argv.[3] |> float
    let retraso = argv.[4] |> int

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
     
    let sufijoUMbral = if modo = "G" then "GJ" else "%"
    let titulo = $"Modo: {modo}\tArchivo: {archivo}\tVentana: {ventana}días,\tUmbral: {umbral}{sufijoUMbral},\tRetraso: {retraso}días"
    printfn "%s" titulo
    let datos = cargarDatos pathArchivo
    

//    printfn $"Modo:{modo}\tArchivo:{archivo}\tVentana:{ventana}\tUmbral:{umbral}\tRetraso:{retraso}"

    let cambios,winMin, winMax = match modo with
                                    | "G" -> getVentanaGJ datos ventana umbral retraso 
                                    | "P" -> getVentana datos ventana umbral retraso 
                                    | _ ->   failwith "Modo de operación no reconocido"
                  
    graficarWin datos cambios winMin winMax titulo
    
    0
