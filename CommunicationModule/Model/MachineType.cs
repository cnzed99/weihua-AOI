using WH.Entity.Attribute;

namespace CommunicationModule
{
    /// <summary>
    /// 2024.7.17 李焕彬
    /// 连接设备类型
    /// </summary>
    public enum EMMACHINETYPE
    {
        /// <summary>
        /// 2024.7.17 李焕彬
        /// MAXWELL
        /// </summary>
        [EnumString("通用", "GENERAL")]
        EMMCGENERAL,
    }

    /// <summary>
    /// 2024.7.17 李焕彬
    /// 转码格式
    /// </summary>
    public enum EMENCODING
    {
        /// <summary>
        /// 2024.7.17 李焕彬
        /// 不转码
        /// </summary>
        [EnumString("NONE", "NONE")]
        EMENCODENONE,

        /// <summary>
        /// 2024.7.17 李焕彬
        /// ASCII
        /// </summary>
        [EnumString("ASCII", "ASCII")]
        EMENCODEASCII,

        /// <summary>
        /// 2024.7.17 李焕彬
        /// UTF8,
        /// </summary>
        [EnumString("UTF8", "UTF8")]
        EMENCODEUTF8,

        /// <summary>
        /// 2024.7.17 李焕彬
        /// UNICODE
        /// </summary>
        [EnumString("UNICODE", "UNICODE")]
        EMENCODEUNICODE,

        /// <summary>
        /// 2024.7.17 李焕彬
        /// DEFAULT
        /// </summary>
        [EnumString("DEFAULT", "DEFAULT")]
        EMENCODEDEFAULT,
    }

    /// <summary>
    /// 2024.7.17 李焕彬
    /// 进制转换类型
    /// </summary>
    public enum EMSYSCONVERT
    {
        /// <summary>
        /// 2024.7.17 李焕彬
        /// DEFAULT
        /// </summary>
        [EnumString("十进制", "DECIMAL")]
        EMCONVERTDECIMAL,

        /// <summary>
        /// 2024.7.17 李焕彬
        /// DEFAULT
        /// </summary>
        [EnumString("十六进制", "HEXADECIMAL")]
        EMCONVERTHEXDECIMAL
    }
}
