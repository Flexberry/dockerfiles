using ICSSoft.STORMNET.KeyGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewPlatform.Flexberry.ORM.ODataService.Tests
{
    public class GUIDToStringGenerator: GUIDGenerator
    {
        /// <summary>
        /// Генерировать Guid
        /// </summary>
        public override object Generate(Type dataObjectType)
        {
            return KeyGuid.NewGuid().Guid.ToString("D");
        }

        /// <summary>
        /// Генерировать Guid
        /// </summary>
        public override object Generate(Type dataObjectType, object sds)
        {
            return KeyGuid.NewGuid().Guid.ToString("D");
        }

        /// <summary>
        /// Генерировать Guid
        /// </summary>
        public override object GenerateUniqe(Type dataObjectType)
        {
            return Generate(dataObjectType);
        }

        /// <summary>
        /// Генерировать Guid
        /// </summary>
        public override object GenerateUniqe(Type dataObjectType, object sds)
        {
            return Generate(dataObjectType, sds);
        }


        /// <summary>
        /// Вернуть тип ключа
        /// </summary>
        public override Type KeyType { get { return typeof(string); } }

        /// <summary>
        /// Уникален ли первичный ключ
        /// </summary>
        public override bool Unique { get { return true; } }
    }
}
