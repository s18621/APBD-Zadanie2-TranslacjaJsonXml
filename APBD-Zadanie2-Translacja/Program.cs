using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace APBD_Zadanie2_Translacja
{
    public class Program
    {
        static void Main(string[] args)
        {

            string[] argumenty = new string[3];
            int arg_len = args.Length;
            switch (arg_len)
            {
                case 0:
                    argumenty[0] = @"dane.csv";
                    argumenty[1] = "result.xml";
                    argumenty[2] = @"xml";
                    break;
                case 1:
                    argumenty[0] = args[0];
                    argumenty[1] = "result.xml";
                    argumenty[2] = @"xml";
                    break;
                case 2:
                    argumenty[0] = args[0];
                    argumenty[1] = args[1];
                    argumenty[2] = @"xml";
                    break;
                case 3:
                    argumenty[0] = args[0];
                    argumenty[1] = args[1];
                    argumenty[2] = args[2];
                    break;
            }

            var StudentHash = new HashSet<Student>(new myComparator());
            Console.WriteLine(argumenty[0]);
            var log = File.Create("log.txt");
            using (StreamWriter logWriter = new StreamWriter(log))
                try
                {
                    using (var stream = new StreamReader(File.OpenRead(argumenty[0])))
                    {                       
                        string line = null;
                        while ((line = stream.ReadLine()) != null)
                        {
                            string[] student = line.Split(',');
                            bool validate = true;
                            for (int i = 0; i < student.Length; i++)
                            {
                                if (student[i].Length <= 1) validate = false;
                            }

                            if (student.Length == 9 && validate)
                            {
                                var st = new Student
                                {
                                    Imie = student[0],
                                    Nazwisko = student[1],
                                    Kierunek = student[2],
                                    Rodzaj = student[3],
                                    IndexNumber = student[4],
                                    rokurodzenia = student[5],
                                    mail = student[6],
                                    imieMatki = student[7],
                                    imieOjca = student[8]
                                };
                                StudentHash.Add(st);
                            }
                            else
                            {
                                Console.WriteLine("Nie mozna dodac studenta\n" + line);

                                logWriter.Write("Nie mozna dodac studenta\n" + line);
                            }
                        }
                    }
                }
                catch(ArgumentException)
                {
                    Console.WriteLine("Podana ścieżka jest niepoprawna");
                    logWriter.Write("Podana ścieżka jest niepoprawna");
                    throw;
                }
                catch (FileNotFoundException Exception)
                {
                    Console.WriteLine("Plik " + argumenty[0] + " nie istnieje");
                    logWriter.Write("Plik " + argumenty[0] + " nie istnieje");

                }

            Dictionary<string, int> count = new Dictionary<string, int>();
            foreach (var student in StudentHash)
            {
                if (!count.ContainsKey(student.Kierunek)) count.Add(student.Kierunek, 1);
                else count[student.Kierunek]++;
            
            }
            if(argumenty[2].Equals("json"))
            {
                FileStream writer = new FileStream(argumenty[1], FileMode.Create);
                using (StreamWriter Jwriter = new StreamWriter(writer))
                {
                    foreach (var student in StudentHash)
                        Jwriter.WriteLine(JsonSerializer.Serialize(student));
                }
            }

            //XML Serializer
            else if (argumenty[2].Equals("xml"))
            {             
                XDocument doc = new XDocument(new XElement("uczelnia",
                    new XAttribute("createdAt", DateTime.Today),
                    new XAttribute("author", "Piotr Lachanski"),
                    new XElement("activeStudies",
                                from c in count
                                select new XElement("studies",
                                    new XAttribute("name", c.Key),
                                    new XAttribute("numberOfStudents", c.Value)
                                    )),
                    new XElement("studenci",
                    from Student in StudentHash
                    select new XElement("student",
                    new XAttribute("indexNumber", Student.IndexNumber),
                    new XElement("fname", Student.Imie),
                    new XElement("lname", Student.Nazwisko),
                    new XElement("birthdate", Student.rokurodzenia),
                    new XElement("email", Student.mail),
                    new XElement("mothersName", Student.imieMatki),
                    new XElement("fathersName", Student.imieOjca),
                    new XElement("studies",
                        new XElement("name", Student.Kierunek),
                        new XElement("mode", Student.Rodzaj)
                        )))));

                doc.Save(argumenty[1]);
            }
        }
        [Serializable]
        [XmlRoot("studenci")]
        public class Student
        { 
            [XmlAttribute("indexNumber")]
            public string IndexNumber { get; set; }


            [XmlElement(ElementName = "fname")]
            public string Imie { get; set; }

            [XmlElement(ElementName = "lname")]
            public string Nazwisko { get; set; }

            [XmlElement(ElementName = "birthdate")]
            public string rokurodzenia { get; set; }

            [XmlElement(ElementName = "email")]
            public string mail { get; set; }

            [XmlElement(ElementName = "mothersName")]
            public string imieMatki { get; set; }

            [XmlElement(ElementName = "fathersName")]
            public string imieOjca { get; set; }

            [XmlElement(ElementName = "name")]
            public string Kierunek { get; set; }

            [XmlElement(ElementName = "mode")]
            public string Rodzaj { get; set; }

        }
        class myComparator : IEqualityComparer<Student>
        {
            public bool Equals(Student x, Student y)
            {
                return StringComparer.InvariantCultureIgnoreCase.Equals($"{x.Imie} {x.Nazwisko} {x.IndexNumber}", $"{y.Imie} {y.Nazwisko} {y.IndexNumber}");
            }

            public int GetHashCode(Student obj)
            {
                return StringComparer.CurrentCultureIgnoreCase.GetHashCode($"{obj.Imie} {obj.Nazwisko} {obj.IndexNumber}");
            }
        }
    }
}
