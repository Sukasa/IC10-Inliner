using static IC10_Inliner.IC10Instruction;

namespace IC10_Inliner
{
    public readonly struct IC10Instruction(string Opcode, params ParameterType[] Params)
    {
        public static readonly IReadOnlyList<IC10Instruction> Instructions = [
            new("hcf"),
            new("sleep", ParameterType.Numeric),
            new("yield"),

            // Hack: allow 'alias' to pass validation
            new("alias", ParameterType.AllowUnknownSymbol | ParameterType.NoSubstitution, ParameterType.AliasTarget),

            new("abs", ParameterType.Register, ParameterType.Numeric),
            new("add", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("ceil", ParameterType.Register, ParameterType.Numeric),
            new("div", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("pow", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("exp", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("floor", ParameterType.Register, ParameterType.Numeric),
            new("log", ParameterType.Register, ParameterType.Numeric),
            new("max", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("min", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("mod", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("move",ParameterType.Register, ParameterType.Numeric),
            new("mul", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("rand", ParameterType.Register),
            new("round", ParameterType.Register, ParameterType.Numeric),
            new("sqrt", ParameterType.Register, ParameterType.Numeric),
            new("sub", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("trunc", ParameterType.Register, ParameterType.Numeric),
            new("lerp", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),

            new("acos", ParameterType.Register, ParameterType.Numeric),
            new("asin", ParameterType.Register, ParameterType.Numeric),
            new("atan", ParameterType.Register, ParameterType.Numeric),
            new("atan2", ParameterType.Register, ParameterType.Numeric),
            new("cos", ParameterType.Register, ParameterType.Numeric),
            new("sin", ParameterType.Register, ParameterType.Numeric),
            new("tan", ParameterType.Register, ParameterType.Numeric),

            new("clr", ParameterType.Device),
            new("get", ParameterType.Register, ParameterType.Device, ParameterType.Numeric),
            new("peek", ParameterType.Register),
            new("poke", ParameterType.Numeric, ParameterType.Numeric),
            new("pop", ParameterType.Register),
            new("push", ParameterType.Numeric),
            new("put", ParameterType.Device, ParameterType.Numeric, ParameterType.Numeric),

            new("l", ParameterType.Register, ParameterType.Device, ParameterType.Enumeration),
            new("lr", ParameterType.Register, ParameterType.Device, ParameterType.Enumeration, ParameterType.Numeric),
            new("ls", ParameterType.Register, ParameterType.Device, ParameterType.Numeric, ParameterType.Enumeration),
            new("s", ParameterType.Device, ParameterType.Enumeration, ParameterType.Numeric),
            new("ss", ParameterType.Device, ParameterType.Numeric, ParameterType.Enumeration, ParameterType.Numeric),
            new("rmap", ParameterType.Register, ParameterType.Device, ParameterType.Numeric),

            new("lb", ParameterType.Register, ParameterType.Numeric, ParameterType.Enumeration, ParameterType.Enumeration),
            new("lbn", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric, ParameterType.Enumeration, ParameterType.Enumeration),
            new("lbns", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric, ParameterType.Enumeration, ParameterType.Enumeration),
            new("lbs", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric, ParameterType.Enumeration, ParameterType.Enumeration),

            new("sb", ParameterType.Numeric, ParameterType.Enumeration, ParameterType.Numeric),
            new("sbn", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Enumeration, ParameterType.Numeric),
            new("sbns", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric, ParameterType.Enumeration, ParameterType.Numeric),
            new("sbs", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Enumeration, ParameterType.Numeric),

            new("and", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("nor", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("not", ParameterType.Register, ParameterType.Numeric),
            new("or", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("sla", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("sll", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("sra", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("srl", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("xor", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("ext", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("ins", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),

            new("select", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("sdns", ParameterType.Register, ParameterType.Device),
            new("sdse", ParameterType.Register, ParameterType.Device),
            new("sap", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("sapz", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("seq", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("seqz", ParameterType.Register, ParameterType.Numeric),
            new("sge", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("sgez", ParameterType.Register, ParameterType.Numeric),
            new("sgt", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("sgtz", ParameterType.Register, ParameterType.Numeric),
            new("sle", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("slez", ParameterType.Register, ParameterType.Numeric),
            new("slt", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("sltz", ParameterType.Register, ParameterType.Numeric),
            new("snan", ParameterType.Register, ParameterType.Numeric),
            new("snanz", ParameterType.Register, ParameterType.Numeric),
            new("sna", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("snaz", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("sne", ParameterType.Register, ParameterType.Numeric, ParameterType.Numeric),
            new("snez", ParameterType.Register, ParameterType.Numeric),

            new("j", ParameterType.Numeric),
            new("jal", ParameterType.Numeric),
            new("jr", ParameterType.Relative),

            new("bdnvl", ParameterType.Device, ParameterType.Enumeration, ParameterType.Numeric),
            new("bdnvs", ParameterType.Device, ParameterType.Enumeration, ParameterType.Numeric),
            new("bdns", ParameterType.Device, ParameterType.Numeric),
            new("bdnsal", ParameterType.Device, ParameterType.Numeric),
            new("bdse", ParameterType.Device, ParameterType.Numeric),
            new("bdseal", ParameterType.Device, ParameterType.Numeric),
            new("brdns", ParameterType.Device, ParameterType.Relative),
            new("brdse", ParameterType.Device, ParameterType.Numeric),
            
            new("bap", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("brap", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric, ParameterType.Relative),
            new("bapal", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("bapz", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("brapz", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Relative),
            new("bapzal", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("beq", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("breq", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Relative),
            new("beqal", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("beqz", ParameterType.Numeric, ParameterType.Numeric),
            new("breqz", ParameterType.Numeric, ParameterType.Relative),
            new("beqzal", ParameterType.Numeric, ParameterType.Numeric),
            new("bge", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("brge", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Relative),
            new("bgeal", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("bgez", ParameterType.Numeric, ParameterType.Numeric),
            new("brgez", ParameterType.Numeric, ParameterType.Relative),
            new("bgezal", ParameterType.Numeric, ParameterType.Numeric),
            new("bgt", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("brgt", ParameterType.Numeric, ParameterType.Numeric,ParameterType.Relative),
            new("bgtal", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("bgtz", ParameterType.Numeric, ParameterType.Numeric),
            new("brgtz", ParameterType.Numeric, ParameterType.Relative),
            new("bgtzal", ParameterType.Numeric, ParameterType.Numeric),
            new("ble", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("brle", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Relative),
            new("bleal", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("blez", ParameterType.Numeric, ParameterType.Numeric),
            new("brlez", ParameterType.Numeric, ParameterType.Relative),
            new("blezal", ParameterType.Numeric, ParameterType.Numeric),
            new("blt", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("brlt", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Relative),
            new("bltal", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("bltz", ParameterType.Numeric, ParameterType.Numeric),
            new("brltz", ParameterType.Numeric, ParameterType.Relative),
            new("bltzal", ParameterType.Numeric, ParameterType.Numeric),
            new("bna", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("brna", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric, ParameterType.Relative),
            new("bnaal", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("bnan", ParameterType.Numeric, ParameterType.Numeric),
            new("brnan", ParameterType.Numeric, ParameterType.Relative),
            new("bnaz", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("brnaz", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Relative),
            new("bnazal", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("bne", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("brne", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Relative),
            new("bneal", ParameterType.Numeric, ParameterType.Numeric, ParameterType.Numeric),
            new("bnez", ParameterType.Numeric, ParameterType.Numeric),
            new("brnez", ParameterType.Numeric, ParameterType.Relative),
            new("bnezal", ParameterType.Numeric, ParameterType.Numeric)
        ];

        public string Mnemonic { get; init; } = Opcode;

        public ParameterType[] Parameters { get; init; } = Params;

        [Flags()]
        public enum ParameterType
        {
            None = 0,
            AllowDevicePin = 1,
            AllowConstant = 2,
            AllowRegister = 4,
            AllowLabel = 8,
            AllowUnknownSymbol = 16,
            NoSubstitution = 32,

            IsDevice = 1,
            IsConstant = 2,
            IsRegister = 4,
            IsUnknownSymbol = 16,

            BranchRelative = 256,

            Register = AllowRegister, // Register only, no constant or device
            Device = AllowDevicePin | AllowRegister | AllowConstant, // After resolve must be a direct or indirect device or register, or constant Id #
            Numeric = AllowConstant | AllowRegister | AllowLabel, // After resolve must be a register or constant symbol (or label), but not direct/indirect device
            Enumeration = AllowConstant | AllowRegister | AllowUnknownSymbol, // Fixed non-symbol string, register, or constant.  Not a direct or indirect device pin.
            Relative = Numeric | BranchRelative, // Like Numeric, but fixed labels are calculated differently,
            AliasTarget = AllowDevicePin | AllowRegister, // Aliases can be device pins or registers
        }
    }
}
