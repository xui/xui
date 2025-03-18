// Set up the .NET WebAssembly runtime
import { dotnet } from './_framework/dotnet.js'

// Get exported methods from the .NET assembly
const { getAssemblyExports, getConfig } = await dotnet
    .withDiagnosticTracing(false)
    .create();

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

// Access JSExport methods using exports.<Namespace>.<Type>.<Method>
const result = exports.Sample.Add(3, 9);

// Display the result of the .NET method
document.getElementById("out").innerHTML = `Result: ${result}`;