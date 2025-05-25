module Autocorrelacion



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



// Calcula la autocorrelación para una ventana movil 10 veces el lag
let autocorrelacionMovil lag (serie: float list)  =
    serie |> List.windowed (lag * 10) |> List.map(fun x -> autocorrelacion x lag)

