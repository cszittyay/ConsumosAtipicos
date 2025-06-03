module zscore
open DataUtils

let detectarAtipicosZS (datos: consumoDiario list) (windowSize: int) (zLimite: float) =
    let rec loop acc (restante:consumoDiario list) =
        match restante with
        | _ when restante.Length < windowSize -> List.rev acc
        | _ ->
            let ventana = restante |> List.take windowSize
            let actual = restante.[windowSize - 1]
            let media = ventana |> List.averageBy (fun x -> x.consumo)
            let desv =
                ventana
                |> List.averageBy (fun x -> pown (x.consumo - media) 2)
                |> sqrt
            let z = (actual.consumo - media) / desv
            let acc' =
                if abs z > zLimite then
                    { actual with consumo = actual.consumo } :: acc
                else acc
            loop acc' (restante.Tail)
    loop [] datos




let graficarZScore (datos: consumoDiario list)  (titulo: string) =
        let cambios = detectarAtipicosZS datos 30 2
        let result = cambios |> List.map (fun x -> { diaGas = x.diaGas; consumo = x.consumo; limite = x.consumo })
        graficar datos result titulo
