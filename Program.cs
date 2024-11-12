//Start application to convert HL7 v2 to FHIR JSON string

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NHapi.Model.V25.Message;
using NHapi.Base.Parser;
using NHapi.Model.V25.Segment;
using NHapi.Base.Model;
using NHapi.Model.V25.Datatype;
using System.Xml.Serialization;
using System.Diagnostics.Metrics;
using System.Reflection.Metadata.Ecma335;

namespace v2toFhir
{
    internal class Program
    {
        static void Main(string[] args) {

            string messageString = @"MSH|^~\&|PARAGON|023|Engine_In_Pat||20241030105047043||ADT^A01|20241030139272152|T|2.5|||AL|NE
EVN|A01|20241030105042.000|||rse00
PID|1|01510213^^^023^MR^PARAGON|01510213^^^023^MR||TEST^DAVID^^^^^L||195210150000|M||W^White^HL70005|5501 SOUTH MCCOLL ROAD^^EDINBURG^TX^78539^US^HOME||(956)362-8677^MOBIL^PH^^1^956^3628677||en^English|M|CAT|16001457^^^023|000-00-0000|||3^Hispanic or Latino|||||Y|||N||||||||20240806
PD1||||""||||U|||N||||N^NONE
NK1|1|TEST^GIZMO|55/G8^Other Relationship|5501 MCCOLL ROAD^^EDINBURG^TX^78539^US^HOME^^48215|(956)362-8677^HOME||EMC^Emergency Contact|20240806|||||||M|197501010000|||||||||||||||^HOME^PH^^1^956^3628677|5501 MCCOLL ROAD^^EDINBURG^TX^78539^US^HOME^^48215|||||000-00-0000
PV1|1|I|2ND^217^01^023|3|||99989^TEST^MD^^^MD~^TEST^MD^^^MD^^^NPPES^^^^NPI|99989^TEST^MD^^^MD~^TEST^MD^^^MD^^^NPPES^^^^NPI||I||||1||N|99989^TEST^MD^^^MD~^TEST^MD^^^MD^^^NPPES^^^^NPI|I|16001457|108||||||||||||||||||||||||202410301047|||||||V
PV2|||^3m test||||||||||||||||||N|N||ADMIT||||||||||||N||PV^PRIVATE VEHICLE
ROL|1|UP|FAMILY^Family Physician|99989^TEST^MD^^^MD|||||PHY^Physician|023^DOCTORS HOSPITAL AT RENAISSANCE
OBX|1|TS|11368-8^ILLNESS ONSET DATE^LN||202410301047||||||F
OBX|2|TX|80427-8^PATIENT EMPLOYER^LN||BLUE WAVE EXPRESS||||||F
GT1|1|2473528|TEST^DAVID||5501 SOUTH MCCOLL ROAD^^EDINBURG^TX^78539^US^HOME|(956)362-8677^MOBIL||195210150000|M|P||000-00-0000|||||1112 N WESTGATE DR^^WESLACO^TX^78556^US^MAILING|(956)731-9904^MAIN|||""|||||||||M||||||en|||||CAT||||||||||BLUE WAVE EXPRESS
IN1|1|MCD|493|MEDICAID|PO BOX 200555^^AUSTIN^TX^78720^USA^MAIN||(800)925-9126^MAIN|||||19600101||||TEST, DAVID|01/18^Self|19521015|5501 SOUTH MCCOLL ROAD^^EDINBURG^TX^78539^US|Y||1|||||Y|||||||||||||||3^Not employed|M||N
IN2||000-00-0000|||||||||||||||||||||||1053317362||||||||||||||||||||||||||||||||||||||(956)362-8677";


            PipeParser parser = new PipeParser();
            IMessage m = parser.Parse(messageString);
            ADT_A01 adt = m as ADT_A01;
            PID pid = adt.PID;
            NK1 nk1 = adt.GetNK1(0);
            PD1 pd1 = adt.PD1;
            var name = pid.GetPatientName(0);
            var familyName = name.FamilyName.Surname;
            var givenName = name.GivenName.Value;

            var phone = pid.GetPhoneNumberHome(0);
            var homePhone = phone.TelephoneNumber;

            var gender = pid.AdministrativeSex;

            var birthDate = pid.DateTimeOfBirth.Time;
            //bool for deceased[x]
            String? result = "0";
            if (pid.PatientDeathIndicator.ToString() == "N")
            {
                result = pid.PatientDeathDateAndTime.Time.ToString();
            } else
            {
                //do nothing
            }
            var deathTime = result;

            var address = pid.GetPatientAddress(0);
            var street = address.StreetAddress.StreetName;
            var city = address.City;
            var zip = address.ZipOrPostalCode;
            var county = address.CountyParishCode;
            var country = address.Country;
            var state = address.StateOrProvince;

            var maritalStatus = pid.MaritalStatus;
            var martial = pid.MaritalStatus.Text;
            //bool for multipleBirth[x]

            var relationship = nk1.Relationship;
            var relCode = nk1.ContactRole;
            var nkFName = nk1.GetName(0).GivenName.Value;
            var nkLName = nk1.GetName(0).FamilyName.Surname;
            var nkNumber = nk1.GetPhoneNumber(0).TelephoneNumber;
            var nkStreet = nk1.GetAddress(0).StreetAddress;
            var nkCity = nk1.GetAddress(0).City;
            var nkZip = nk1.GetAddress(0).ZipOrPostalCode;
            var nkCounty = nk1.GetAddress(0).CountyParishCode;
            var nkCountry = nk1.GetAddress(0).Country;
            var nkState = nk1.GetAddress(0).StateOrProvince;
            var nkGender = nk1.AdministrativeSex;
            var nkOrg = nk1.GetOrganizationNameNK1(0).OrganizationName;
            var nkSysOrg = nk1.GetOrganizationNameNK1(0).AssigningFacility;

            var lang = pid.PrimaryLanguage;

            var dr = pd1.GetPatientPrimaryCareProviderNameIDNo(0).IDNumber;


            string jsonData = @"{'Value':'" + nkNumber + "'}";

            string patientJson = $@"
{{
  ""resourceType"": ""Patient"",
  ""identifier"": [
    {{
      ""use"": ""usual"",
      ""type"": {{
        ""coding"": [
          {{
            ""system"": ""http://terminology.hl7.org/CodeSystem/v2-0203"",
            ""code"": ""{pid.PatientID[0]}"",
            ""display"": ""Patient Identifier""
          }}
        ],
        ""text"": ""{pid.PatientID[0]}""
      }},
    }}
  ],
  ""active"": ""true"",
  ""name"": [
    {{
      ""use"": ""official"",
      ""family"": ""{familyName}"",
      ""given"": [
        ""{givenName}""
      ],
    }}
  ],
  ""telecom"": [
    {{
      ""system"": ""phone"",
      ""value"": ""{homePhone}"",
      ""use"": ""home""
    }},
  ],
  ""gender"": ""{gender}"",
  ""birthDate"": ""{birthDate}"",
  ""deceasedBoolean"": {pid.PatientDeathIndicator},
  ""deceasedDateTime"": ""{deathTime}"",
  ""address"": [
    {{
      ""use"": ""home"",
      ""type"": ""both"",
      ""text"": ""{street}"",
      ""line"": [
        ""{street}""
      ],
      ""city"": ""{city}"",
      ""district"": ""{county}"",
      ""state"": ""{state}"",
      ""postalCode"": ""{zip}"",
      ""country"": ""{country}""
    }}
  ],
  ""maritalStatus"": {{
    ""coding"": [
      {{
        ""system"": ""http://terminology.hl7.org/CodeSystem/v3-MaritalStatus"",
        ""code"": ""{maritalStatus.Text}"",
        ""display"": ""{martial}""
      }}
    ]
  }},
  ""contact"": [
    {{
      ""relationship"": [
        {{
          ""coding"": [
            {{
              ""system"": ""http://terminology.hl7.org/CodeSystem/v2-0163"",
              ""code"": ""{relCode.Text}"",
              ""display"": ""{relationship.Text}""
            }}
          ],
          ""text"": ""{relationship.Text}""
        }}
      ],
      ""name"": {{
        ""use"": ""usual"",
        ""family"": ""{nkLName}"",
        ""given"": [
          ""{nkFName}""
        ],
      }},
      ""telecom"": [
        {{
          ""system"": ""phone"",
          ""value"": ""{nkNumber}"",
          ""use"": ""home""
        }},
      ],
      ""address"": {{
        ""use"": ""home"",
        ""type"": ""both"",
        ""text"": ""{nkStreet.StreetName}"",
        ""line"": [
          ""{nkStreet.StreetName}""
        ],
        ""city"": ""{nkCity}"",
        ""district"": ""{nkCounty}"",
        ""state"": ""{nkState}"",
        ""postalCode"": ""{nkZip}"",
        ""country"": ""{nkCountry}""
      }},
      ""gender"": ""{nkGender}"",
      ""organization"": {{
        ""reference"": ""{nkOrg}"",
        ""type"": ""Organization"",
        ""identifier"": {{
          ""use"": ""official"",
          ""type"": {{
            ""coding"": [
              {{
                ""system"": ""http://terminology.hl7.org/CodeSystem/v2-0203"",
                ""code"": ""NPI"",
                ""display"": ""National Provider Identifier""
              }}
            ],
            ""text"": ""NPI""
          }},
          ""system"": ""{nkSysOrg.NamespaceID}"",
          ""value"": ""{nkOrg}""
        }},
        ""name"": ""{nkOrg}""
      }},
    }}
  ],
  ""communication"": [
    {{
      ""language"": {{
        ""coding"": [
          {{
            ""system"": ""urn:ietf:bcp:47"",
            ""code"": ""{lang.Text}"",
            ""display"": ""{lang.NameOfCodingSystem}""
          }}
        ],
        ""text"": ""English""
      }},
      ""preferred"": 1
    }}
  ],
  ""generalPractitioner"": [
    {{
      ""reference"": ""{dr}"",
      ""type"": ""Practitioner""
    }}
  ],
  ""link"": [
    {{
      ""other"": {{git
        ""type"": ""Patient""
      }},
      ""type"": ""replace""
    }}
  ]
}}";


            try
            {
                //exception is thrown without the Trim(); No exception with the Trim()
                //var ourHL7Message = ourPP.Parse(messageString.Trim());

                Console.WriteLine(patientJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();

        }
    }
}