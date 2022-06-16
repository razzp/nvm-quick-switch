$app_version = "1.2.0"
$root = [string](Get-Location)
$publish_dir = $root + "\NVMQuickSwitch\bin\Release\net6.0-windows\publish"
$output_dir = $root + "\release"
$destination_path = $output_dir + "\NVMQuickSwitch-" + $app_version + ".zip"

if (Test-Path -Path $publish_dir) {
	if (-not(Test-Path -Path $output_dir)) {
		New-Item $output_dir -ItemType Directory | Out-Null
	}
	
	Compress-Archive -Path ($publish_dir + "\*") -DestinationPath $destination_path -Force
	
	Write-Host "ZIP file created successfully" -ForegroundColor Green
} else {
	Write-Host "Cannot find publish directory. Have you published?" -ForegroundColor Red
}