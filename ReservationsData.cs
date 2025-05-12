using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionAPIApp
{
    [JsonObject]
    public class ReservationsList
    {
        [JsonProperty("List")]
        public List<ReservationsRow> List { get; set; } = new List<ReservationsRow>();
    }

    [JsonObject]
    public class ReservationsRow
    {
        [JsonProperty("ID")]
        public int ID { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("PhoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("MailAddress")]
        public string MailAddress{ get; set; }

        [JsonProperty("Number_of_Tickets")]
        public int Number_of_Tickets{ get; set; }

        [JsonProperty("SeatType")]
        public string SeatType { get; set; }


    }
}