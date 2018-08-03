using System;
namespace DavLib
{
    public enum OpCodes
    {
        nop = 0,
        jmp,
        jnz,
        jzo,
        ret,
        call,
        or = 0x10,
        xor,
        and,
        not,
        add = 0x20,
        sub,
        mul,
        mod,
        lsh,
        rsh,
        eql = 0x30,
        gra,
        les,
        psh = 0x40,
        pop,
        lod,
        sto,
        mov,
        set,
        lodi = 0x51,
        stoi,
        jmpi,
        jnzi,
        jzoi,
        cali,
        pgm = 0x60
    }
}
