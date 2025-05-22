open Plotly.NET
open Caso01

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"



// === PARTE 4: Ejecución ===   

//let datos = cargarDatos "Consumos.csv"
//let cambios = detectarCambios datos 7 30.0
//graficar datos cambios 

// CSUM


[<EntryPoint>]
let main argv =
    let pathBase = @"C:\Users\cszit\source\repos\f#\MX\ConsumosAtipicos\ConsumosAtipicos\"
    let pathArchivo =  pathBase + @"ConsumosAtipicos02.csv"
    let datos = cargarDatos pathArchivo
    //let avg = 40.0
    //let umbral = 20.0
    //let cambios = detectarCambiosCUSUM datos avg umbral
    //graficarCUSUM datos cambios

    // analisis de los argumentos de entrada
    printfn $"Archivo: {pathArchivo}"

    let modo = argv.[0]
    let ventana = argv.[1] |> int
    let umbral = argv.[2] |> float
    let retraso = argv.[3] |> int


    printfn $"Modo:{modo}\tVentana:{ventana}\tUmbral:{umbral}\tRetraso:{retraso}"

    match modo with
    | "G" -> getVentanaGJ datos ventana umbral retraso 
    | "P" -> getVentana datos ventana umbral retraso 
    
    0