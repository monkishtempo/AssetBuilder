$dir = "apps_expert-24_com\ABTemp"
#$dir = "APH\AssetBuilder"
#$dir = "apps_expert-24_com\WestField2016\"
$source = "\\tsclient\C\wwwroot\$dir"
$dest = "F:\wwwroot\$dir"
$appfilesource = "$source\Application Files"
$appfilesdest = "$dest\Application Files"

pushd $source
dir
robocopy . $dest
$latest = (Get-ChildItem $appfilesource | Where { $_.PSIsContainer } | Sort CreationTime -Descending | Select -First 1)
$latest
$appfilesource+="\$(${$latest}.Name)"
$appfilesdest+="\$(${$latest}.Name)"

robocopy $appfilesource $appfilesdest /e /MIR

popd