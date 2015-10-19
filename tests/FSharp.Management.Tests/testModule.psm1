function doSomething {
    [OutputType([string])]
    param (
        [string] $test
    )
    return $test
}

export-moduleMember -function doSomething