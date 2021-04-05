namespace ExchangeRatesAPI.Models
{
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.gesmes.org/xml/2002-08-01")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.gesmes.org/xml/2002-08-01", IsNullable = false)]
    public partial class Envelope
    {
        private string subjectField;
        private EnvelopeSender senderField;
        private CubeCube[] cubeField;
        public string subject
        {
            get
            {
                return this.subjectField;
            }
            set
            {
                this.subjectField = value;
            }
        }

        public EnvelopeSender Sender
        {
            get
            {
                return this.senderField;
            }
            set
            {
                this.senderField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayAttribute(Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
        [System.Xml.Serialization.XmlArrayItemAttribute("Cube", IsNullable = false)]
        public CubeCube[] Cube
        {
            get
            {
                return this.cubeField;
            }
            set
            {
                this.cubeField = value;
            }
        }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.gesmes.org/xml/2002-08-01")]
    public partial class EnvelopeSender
    {
        private string nameField;
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
    public partial class CubeCube
    {
        private CubeCubeCube[] cubeField;
        private System.DateTime timeField;

        [System.Xml.Serialization.XmlElementAttribute("Cube")]
        public CubeCubeCube[] Cube
        {
            get
            {
                return this.cubeField;
            }
            set
            {
                this.cubeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
    public partial class CubeCubeCube
    {
        private string currencyField;
        private decimal rateField;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string currency
        {
            get
            {
                return this.currencyField;
            }
            set
            {
                this.currencyField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal rate
        {
            get
            {
                return this.rateField;
            }
            set
            {
                this.rateField = value;
            }
        }
    }

    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref", IsNullable = false)]
    public partial class Cube
    {
        private CubeCube[] cube1Field;

        [System.Xml.Serialization.XmlElementAttribute("Cube")]
        public CubeCube[] Cube1
        {
            get
            {
                return this.cube1Field;
            }
            set
            {
                this.cube1Field = value;
            }
        }
    }
}