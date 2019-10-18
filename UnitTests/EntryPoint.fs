module EntryPoint

let _ = 0

// F# adds an entry point for executable projects to the last file.
// If the last file is a test module, it messes up static initializers.
