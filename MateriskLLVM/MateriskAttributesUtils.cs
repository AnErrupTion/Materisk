namespace MateriskLLVM;

public static class MateriskAttributesUtils
{
    public static MateriskAttributes CreateAttributes(bool isPublic, bool isStatic, bool isNative)
    {
        var attributes = MateriskAttributes.None;

        if (isPublic)
            attributes |= MateriskAttributes.Public;

        if (isStatic)
            attributes |= MateriskAttributes.Static;

        if (isNative)
            attributes |= MateriskAttributes.Native;

        return attributes;
    }
}