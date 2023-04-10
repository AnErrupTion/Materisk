namespace Materisk.TypeSystem;

public static class MateriskAttributesUtils
{
    public static MateriskAttributes CreateAttributes(bool isPublic, bool isStatic, bool isNative, bool isExternal, bool isStruct)
    {
        var attributes = MateriskAttributes.None;

        if (isPublic) attributes |= MateriskAttributes.Public;
        if (isStatic) attributes |= MateriskAttributes.Static;
        if (isNative) attributes |= MateriskAttributes.Native;
        if (isExternal) attributes |= MateriskAttributes.External;
        if (isStruct) attributes |= MateriskAttributes.Struct;

        return attributes;
    }
}