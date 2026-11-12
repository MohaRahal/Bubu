param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot "Previews")
)

$ErrorActionPreference = "Stop"
$layout = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot "_layout.html")
$wwwrootPath = Join-Path (Split-Path $PSScriptRoot -Parent) "wwwroot"

function Get-ImageDataUri([string]$RelativePath) {
    $bytes = [System.IO.File]::ReadAllBytes((Join-Path $wwwrootPath $RelativePath))
    return "data:image/png;base64,$([Convert]::ToBase64String($bytes))"
}

$brandIcon = Get-ImageDataUri "avatars\bubu-classico.png"

$previews = @(
    @{
        File = "wallet-income.html"
        Variables = @{
            UserName = "Maria"
            MovementLabel = "Entrada"
            AccountName = "Carteira principal"
            Description = "Pagamento recebido"
            Amount = "R$ 1.250,00"
            TransactionDate = "09/11/2026"
            PreviewText = "Uma entrada de R$ 1.250,00 foi adicionada à sua carteira."
            BrandIconUrl = $brandIcon
            MascotUrl = Get-ImageDataUri "email-assets\bubu-guardando-dinheiro.png"
            CurrentYear = "2026"
        }
    },
    @{
        File = "wallet-expense.html"
        Variables = @{
            UserName = "Maria"
            MovementLabel = "Saída"
            AccountName = "Carteira principal"
            Description = "Compra no supermercado"
            Amount = "R$ 287,40"
            TransactionDate = "09/11/2026"
            PreviewText = "Uma saída de R$ 287,40 foi registrada na sua carteira."
            BrandIconUrl = $brandIcon
            MascotUrl = Get-ImageDataUri "email-assets\bubu-tirando-dinheiro.png"
            CurrentYear = "2026"
        }
    },
    @{
        File = "password-changed.html"
        Variables = @{
            UserName = "Maria"
            ChangedAt = "09/11/2026 às 14:30"
            PreviewText = "A senha da sua conta Bubu foi alterada."
            BrandIconUrl = $brandIcon
            MascotUrl = $brandIcon
            CurrentYear = "2026"
        }
    },
    @{
        File = "password-reset-code.html"
        Variables = @{
            UserName = "Maria"
            Code = "482731"
            LifetimeMinutes = "10"
            PreviewText = "Seu código de recuperação Bubu é 482731."
            BrandIconUrl = $brandIcon
            MascotUrl = $brandIcon
            CurrentYear = "2026"
        }
    }
)

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

foreach ($preview in $previews) {
    $content = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot $preview.File)
    $html = $layout.Replace("{{Content}}", $content)

    foreach ($variable in $preview.Variables.GetEnumerator()) {
        $encodedValue = [System.Net.WebUtility]::HtmlEncode([string]$variable.Value)
        $html = $html.Replace("{{$($variable.Key)}}", $encodedValue)
    }

    $destination = Join-Path $OutputDirectory $preview.File
    Set-Content -LiteralPath $destination -Value $html -Encoding utf8
}

Write-Output "Prévias geradas em $OutputDirectory"
