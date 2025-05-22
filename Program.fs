open Plotly.NET
open Caso01

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"





// === PARTE 4: Ejecución ===   
let pathBase = @"C:\Users\cszit\source\repos\f#\MX\ConsumosAtipicos\ConsumosAtipicos\"
let archivo =  $"ConsumosAtipicos06"
let pathArchivo =  pathBase + archivo + ".csv" // @"ConsumosAtipicos02.csv"
//    


//let datos = cargarDatos pathArchivo 
//let cambios = getVentanaGJ datos 10 100 2 |> Seq.map (fun (x, y, z) -> x) |> Seq.toList
//graficar datos cambios  


let datos = cargarDatos pathArchivo 
let cambios = getVentana datos 10 20 2 |> Seq.map (fun (x, y, z) -> x) |> Seq.toList
graficar datos cambios  



// datos |> List.map(fun x -> x.consumo) |> List.toArray |> detectarFrecuencias


//let datos = cargarDatos pathArchivo |> List.map(fun x -> x.consumo)
//autocorrelacion datos 6 |> printfn "%f" 

//[<EntryPoint>]
//let main argv =
//    //let avg = 40.0
//    //let umbral = 20.0
//    //let cambios = detectarCambiosCUSUM datos avg umbral
//    //graficarCUSUM datos cambios

//    // analisis de los argumentos de entrada
    
//    let modo = argv.[0]
//    let archivo = argv.[1]      // "s##"
//    let ventana = argv.[2] |> int
//    let umbral = argv.[3] |> float
//    let retraso = argv.[4] |> int

//    let pathBase = @"C:\Users\cszit\source\repos\f#\MX\ConsumosAtipicos\ConsumosAtipicos\"
//    let archivo =  $"ConsumosAtipico{archivo}"
//    let pathArchivo =  pathBase + archivo + ".csv" // @"ConsumosAtipicos02.csv"
//    let datos = cargarDatos pathArchivo
    

//    printfn $"Modo:{modo}\tArchivo:{archivo}\tVentana:{ventana}\tUmbral:{umbral}\tRetraso:{retraso}"

//    match modo with
//    | "G" -> getVentanaGJ datos ventana umbral retraso 
//    | "P" -> getVentana datos ventana umbral retraso 
    
//    0