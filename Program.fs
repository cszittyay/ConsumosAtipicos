open Plotly.NET
open Caso01

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"



// === PARTE 4: Ejecución ===   

//let datos = cargarDatos "Consumos.csv"
//let cambios = detectarCambios datos 7 30.0
//graficar datos cambios 

// CSUM

let datos = cargarDatos "ConsumosAtipicos01.csv"
//let avg = 40.0
//let umbral = 20.0
//let cambios = detectarCambiosCUSUM datos avg umbral
//graficarCUSUM datos cambios

getVenanta datos 7 |> ignore

// 