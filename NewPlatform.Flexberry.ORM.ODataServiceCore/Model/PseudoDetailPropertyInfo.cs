namespace NewPlatform.Flexberry.ORM.ODataService.Model
{
    using System;
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    /// Реализация <see cref="PropertyInfo" />, представляющая связь от мастера к псевдодетейлу (псевдосвойство).
    /// </summary>
    public class PseudoDetailPropertyInfo : PropertyInfo
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="masterToDetailPseudoProperty">Имя связи от мастера к псевдодетейлу (псевдосвойство).</param>
        /// <param name="pseudoPropertyType">Тип псевдосвойства.</param>
        /// <param name="masterType">Тип мастера.</param>
        public PseudoDetailPropertyInfo(string masterToDetailPseudoProperty, Type pseudoPropertyType, Type masterType)
        {
            _name = masterToDetailPseudoProperty;
            _type = pseudoPropertyType;
            _declaringType = masterType;
        }

        private readonly string _name;

        private readonly Type _type;

        private readonly Type _declaringType;

        /// <summary>
        /// PropertyType.
        /// </summary>
        public override Type PropertyType => _type;

        /// <summary>
        /// Attributes.
        /// </summary>
        public override PropertyAttributes Attributes => throw new NotImplementedException();

        /// <summary>
        /// CanRead.
        /// </summary>
        public override bool CanRead => throw new NotImplementedException();

        /// <summary>
        /// CanWrite.
        /// </summary>
        public override bool CanWrite => throw new NotImplementedException();

        /// <summary>
        /// Name.
        /// </summary>
        public override string Name => _name;

        /// <summary>
        /// DeclaringType.
        /// </summary>
        public override Type DeclaringType => _declaringType;

        /// <summary>
        /// ReflectedType.
        /// </summary>
        public override Type ReflectedType => throw new NotImplementedException();

        /// <summary>
        /// GetAccessors.
        /// </summary>
        /// <param name="nonPublic">nonPublic</param>
        /// <returns>MethodInfo array</returns>
        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetCustomAttributes.
        /// </summary>
        /// <param name="inherit">inherit</param>
        /// <returns>object array</returns>
        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetCustomAttributes.
        /// </summary>
        /// <param name="attributeType">attributeType</param>
        /// <param name="inherit">inherit</param>
        /// <returns>object array</returns>
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetGetMethod.
        /// </summary>
        /// <param name="nonPublic">nonPublic</param>
        /// <returns>MethodInfo</returns>
        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetIndexParameters.
        /// </summary>
        /// <returns>ParameterInfo array</returns>
        public override ParameterInfo[] GetIndexParameters()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetSetMethod.
        /// </summary>
        /// <param name="nonPublic">nonPublic</param>
        /// <returns>MethodInfo</returns>
        public override MethodInfo GetSetMethod(bool nonPublic)
        {
           throw new NotImplementedException();
        }

        /// <summary>
        /// GetValue.
        /// </summary>
        /// <param name="obj">obj</param>
        /// <param name="invokeAttr">invokeAttr</param>
        /// <param name="binder">binder</param>
        /// <param name="index">index</param>
        /// <param name="culture">culture</param>
        /// <returns>object</returns>
        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// IsDefined.
        /// </summary>
        /// <param name="attributeType">attributeType</param>
        /// <param name="inherit">inherit</param>
        /// <returns>bool</returns>
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// SetValue.
        /// </summary>
        /// <param name="obj">obj</param>
        /// <param name="value">value</param>
        /// <param name="invokeAttr">invokeAttr</param>
        /// <param name="binder">binder</param>
        /// <param name="index">index</param>
        /// <param name="culture">culture</param>
        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
