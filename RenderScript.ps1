$file = $args[0]

#Const path, redo if needed
Set-Alias renderer D:\Development\Uni\Lightfieldretrieval\Lightfieldretrieval\bin\x86\Debug\Lightfieldretrieval.exe

if ($file)
{
    # File provided
    echo("Processing file " + $file)
    $dir = (get-item $file).directoryname;
    $basenames = Get-Content($file)
    foreach($model in $basenames) {
        $modelpath = $dir + "\" + $model
        echo("Rendering " + $modelpath)
        renderer $modelpath
        echo("Done rendering " + $modelpath)
    }    
    echo("Done processing file " + $file)
}
else
{
    echo("No input file!")
}

