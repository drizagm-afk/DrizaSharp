using DrzSharp.Compiler.Virtual;

namespace DrzSharp.Compiler;

interface APITEST
{
    //>>>> CONST
    void Int32(int value);
    void Int64(long value);
    void Float32(float value);
    void Float64(double value);
    void String(string value);
    void Null();

    //>>>> FLOW
    void EnterMethod(int methodId);
    void ExitMethod();
    void Return();

    //BRANCH
    void Label(int labelId);
    void Goto(int labelId);
    void GotoIfTrue(int labelId);
    void GotoIfFalse(int labelId);

    //>>>> MATH
    //ARITHMETIC
    void Add();
    void Sub();
    void Neg();
    void Mul();
    void Div();
    void DivUnsigned();
    void Rem();
    void RemUnsinged();

    //BITWISE
    void And();
    void Or();
    void Xor();
    void Not();
    void ShiftLeft();
    void ShiftRight();

    //COMPARE
    void Equal();
    void GreaterThan();
    void GreaterThanUnsigned();
    void LessThan();
    void LessThanUnsigned();

    //>>>> STACK
    void Dup();
    void Pop();

    //>>> CALL
    void Call(int methodId);
    void CallVirt(int methodId);
    void NewObject(int ctorId);

    //>>>> TYPE
    //STRUCT
    void Unbox(UType type);
    void UnboxAddress(UType type);
    void Box(UType type);

    //ARRAY
    void NewArray(UType utype);
    void LoadLength();
    void LoadElement(UType utype);
    void LoadElementAddress(UType utype);
    void StoreElement(UType utype);

    //CAST
    void CastTo(UType utype);
    void TryCastTo(UType type);

    //ADDRESS
    void LoadFromAddress(UType type);
    void StoreAtAddress(UType type);
    void InitAtAddress(UType utype);

    //>>>> STORAGE
    //LOCAL
    void LoadLocal(int varId);
    void LoadLocalAddress(int varId);
    void StoreLocal(int varId);
    void DeclLocal(int varId, UType utype);

    //ARG
    void LoadArg(int argId);
    void LoadArgAddress(int argId);
    void StoreArg(int argId);

    //FIELD
    void LoadField(int fieldId);
    void LoadFieldAddress(int fieldId);
    void StoreField(int fieldId);
}