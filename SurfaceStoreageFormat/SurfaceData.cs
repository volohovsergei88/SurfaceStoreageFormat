using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SurfaceStoreageFormat.AllClass;

namespace SurfaceStoreageFormat
{
   public class SurfaceData
    {
        public Header header { get; set; }
        public TablePokr Covers { get; set; }
        public List<Piket> Pikets { get; set; }
        public TreangleCluster Clr { get; set; }
        public GlobalTriangleTable GlobalTriangles { get; set; }


       public static SurfaceData ReadFileData(byte[] fileData)
        {

            // Парсинг заголовка
            int offset = 0;
            Header header =Program.ParceHeader(fileData, ref offset);

            // Парсинг таблицы покрытий
            TablePokr coverTable = Program.ParcePokr(fileData, ref offset);
            //Парсинг отметок и треугольников
           List<Piket> pk = Program.ParsePikets(fileData, ref offset);
            //Парсинг треугольников внутри кластера
            TreangleCluster clr = Program.TrgClust(fileData, ref offset);

            //Парсинг треугольников в разных кластерах
            GlobalTriangleTable glTr = Program.ParceGlTrg(fileData, ref offset);

            SurfaceData parsedData = new SurfaceData
            {
                header = header,
                Covers = coverTable,
                Pikets = pk,
                Clr=clr,
                GlobalTriangles = glTr
            };
            return parsedData;
        }
    }
}
