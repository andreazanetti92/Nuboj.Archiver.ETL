using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuboj.Archiver.ETL.Saver.Models
{
    public class ComponentInfo
    {
        // Guid aggiunto da Andrea Zanetti come PK
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SensorDataType { get; set; }
        public int fr9_n_compatibilita { get; set; }
        public string fr9_vs_app { get; set; }
        public string fr9_nome_app { get; set; }
        public string fr9_vs_bootlander { get; set; }
        public string fr9_nome_bootlander { get; set; }
        public string fr9_nome_scheda { get; set; }
        public string fr9_sn { get; set; }
        public int fr8_n_compatibilita { get; set; }
        public string fr8_vs_app { get; set; }
        public string fr8_nome_app { get; set; }
        public string fr8_vs_bootlander { get; set; }
        public string fr8_nome_bootlander { get; set; }
        public string fr8_nome_scheda { get; set; }
        public string fr8_sn { get; set; }
        public int fr7_n_compatibilita { get; set; }
        public string fr7_vs_app { get; set; }
        public string fr7_nome_app { get; set; }
        public string fr7_vs_bootlander { get; set; }
        public string fr7_nome_bootlander { get; set; }
        public string fr7_nome_scheda { get; set; }
        public string fr7_sn { get; set; }
        public int fr6_n_compatibilita { get; set; }
        public string fr6_vs_app { get; set; }
        public string fr6_nome_app { get; set; }
        public string fr6_vs_bootlander { get; set; }
        public string fr6_nome_bootlander { get; set; }
        public string fr6_nome_scheda { get; set; }
        public string fr6_sn { get; set; }
        public int fr5_n_compatibilita { get; set; }
        public string fr5_vs_app { get; set; }
        public string fr5_nome_app { get; set; }
        public string fr5_vs_bootlander { get; set; }
        public string fr5_nome_bootlander { get; set; }
        public string fr5_nome_scheda { get; set; }
        public string fr5_sn { get; set; }
        public int fr4_n_compatibilita { get; set; }
        public string fr4_vs_app { get; set; }
        public string fr4_nome_app { get; set; }
        public string fr4_vs_bootlander { get; set; }
        public string fr4_nome_bootlander { get; set; }
        public string fr4_nome_scheda { get; set; }
        public string fr4_sn { get; set; }
        public int fr3_n_compatibilita { get; set; }
        public string fr3_vs_app { get; set; }
        public string fr3_nome_app { get; set; }
        public string fr3_vs_bootlander { get; set; }
        public string fr3_nome_bootlander { get; set; }
        public string fr3_nome_scheda { get; set; }
        public string fr3_sn { get; set; }
        public int fr2_n_compatibilita { get; set; }
        public string fr2_vs_app { get; set; }
        public string fr2_nome_app { get; set; }
        public string fr2_vs_bootlander { get; set; }
        public string fr2_nome_bootlander { get; set; }
        public string fr2_nome_scheda { get; set; }
        public string fr2_sn { get; set; }
        public int fr1_n_compatibilita { get; set; }
        public string fr1_vs_app { get; set; }
        public string fr1_nome_app { get; set; }
        public string fr1_vs_bootlander { get; set; }
        public string fr1_nome_bootlander { get; set; }
        public string fr1_nome_scheda { get; set; }
        public string fr1_sn { get; set; }
        public int fr0_n_compatibilita { get; set; }
        public string fr0_vs_app { get; set; }
        public string fr0_nome_app { get; set; }
        public string fr0_vs_bootlander { get; set; }
        public string fr0_nome_bootlander { get; set; }
        public string fr0_nome_scheda { get; set; }
        public string fr0_sn { get; set; }
        public int mc_n_compatibilita { get; set; }
        public string mc_vs_app { get; set; }
        public string mc_nome_app { get; set; }
        public string mc_vs_bootlander { get; set; }
        public string mc_nome_bootlander { get; set; }
        public string mc_nome_scheda { get; set; }
        public string mc_sn { get; set; }
        public int sm_n_compatibilita { get; set; }
        public string sm_vs_app { get; set; }
        public string sm_nome_app { get; set; }
        public string sm_vs_bootlander { get; set; }
        public string sm_nome_bootlander { get; set; }
        public string sm_nome_scheda { get; set; }
        public string sm_sn { get; set; }
        public long SensorDataTimestamp { get; set; }
        public string UniqueCode { get; set; }
        public string TenantId { get; set; }
    }
}
