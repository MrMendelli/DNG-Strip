
using System;
using System.Collections.Generic;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

namespace DNGuard
{
    class main
    {
        static void Main(string[] args)
        {
            ModuleDefMD module = ModuleDefMD.Load(args[0]);
            foreach (TypeDef type in module.Types)
            {
                if (!type.HasMethods) continue;
                foreach (MethodDef method in type.Methods)
                {
                    if (method.HasBody && method.Body.HasInstructions)
                    {
                        IList<Instruction> IL = method.Body.Instructions;
                        for (int i = 0; i < IL.Count; i++)
                        {
                           if (IL[i].OpCode == OpCodes.Call && IL[i].Operand == null && IL[i + 3].OpCode == OpCodes.Br_S)
                            {
                                Console.WriteLine($"Invalid method: {method.Name} ({i})");
                                IL[i].OpCode = OpCodes.Nop;
                                IL[i + 3].OpCode = OpCodes.Nop;
                            }
                        }
                    }
                }
            }

            ModuleWriterOptions ee = new ModuleWriterOptions(module);
            ee.MetadataLogger = DummyLogger.NoThrowInstance;
            string outfile = Path.GetFileNameWithoutExtension(module.FullName);
            module.Write($@"{Environment.CurrentDirectory}\{outfile} (Deobfuscated)" + ".exe", ee);
        }
    }
}
