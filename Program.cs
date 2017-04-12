using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace PrepareTeachers
{
    class Program
    {
        const string s_TeachersSchemaLocation = @"C:\Users\Sasha\Documents\Visual Studio 2015\Projects\RecordKeeperFresh\RecordKeeper\SchemaTeachers.csv";
        const string s_TeachersLocation = @"C:\Users\Sasha\Documents\Sagalingua\RecordKeeper\Live\Teachers\Teachers.csv";
        const string s_TeachersPrepLocation = @"C:\Users\Sasha\Documents\Sagalingua\RecordKeeper\Prepared\Teachers.csv";

        const string s_StudentsSchemaLocation = @"C:\Users\Sasha\Documents\Visual Studio 2015\Projects\RecordKeeperFresh\RecordKeeper\SchemaStudents.csv";
        const string s_StudentsLocation = @"C:\Users\Sasha\Documents\Sagalingua\RecordKeeper\Live\Students\Students.csv";
        const string s_StudentsPrepLocation = @"C:\Users\Sasha\Documents\Sagalingua\RecordKeeper\Prepared\Students.csv";

        const string s_LessonsSchemaLocation = @"C:\Users\Sasha\Documents\Visual Studio 2015\Projects\RecordKeeperFresh\RecordKeeper\SchemaLessons.csv";
        const string s_LessonsLocation = @"C:\Users\Sasha\Documents\Sagalingua\RecordKeeper\Live\Lessons\Lessons.csv";
        const string s_LessonsPrepLocation = @"C:\Users\Sasha\Documents\Sagalingua\RecordKeeper\Prepared\Lessons.csv";

        const string s_ProgramsSchemaLocation = @"C:\Users\Sasha\Documents\Visual Studio 2015\Projects\RecordKeeperFresh\RecordKeeper\SchemaPrograms.csv";
        const string s_ProgramsLocation = @"C:\Users\Sasha\Documents\Sagalingua\RecordKeeper\Live\Programs\Programs.csv";
        const string s_ProgramsPrepLocation = @"C:\Users\Sasha\Documents\Sagalingua\RecordKeeper\Prepared\Programs.csv";

        static void Main(string[] args)
        {
            PrepTeachers();
            PrepStudents();
            PrepLessons();
            PrepPrograms();
        }

        static void PrepPrograms()
        {
            if (File.Exists(s_ProgramsPrepLocation))
                File.Delete(s_ProgramsPrepLocation);

            File.Copy(s_ProgramsLocation, s_ProgramsPrepLocation);
        }

        static void PrepStudents()
        {
            PrepPeople(s_StudentsSchemaLocation, s_StudentsLocation, s_StudentsPrepLocation, "StudentName");
        }

        static void PrepTeachers()
        {
            PrepPeople(s_TeachersSchemaLocation, s_TeachersLocation, s_TeachersPrepLocation, "TeacherName");
        }

        static void PrepLessons()
        {
            string[] schema = File.ReadAllLines(s_LessonsSchemaLocation);
            int indDay = FindField(schema, "Day", 0);
            Debug.Assert(indDay >= 0);
            int indStart = FindField(schema, "Start", 0);
            Debug.Assert(indStart >= 0);
            int indEnd = FindField(schema, "End", 0);
            Debug.Assert(indEnd >= 0);
            int indTeacher1 = FindField(schema, "Teacher1", 0);
            Debug.Assert(indTeacher1 >= 0);
            int indTeacher2 = FindField(schema, "Teacher2", 0);
            Debug.Assert(indTeacher2 >= 0);
            int indStudent1 = FindField(schema, "Student1", 0);
            Debug.Assert(indStudent1 >= 0);
            int indStudent10 = FindField(schema, "Student10", 0);
            Debug.Assert(indStudent10 >= 0 && indStudent10 == indStudent1 + 9);

            if (File.Exists(s_LessonsPrepLocation))
                File.Delete(s_LessonsPrepLocation);

            string[] data = File.ReadAllLines(s_LessonsLocation);
            int rows = data.Length;
            Debug.Assert(rows > 1);
            using (System.IO.StreamWriter swPrep = new System.IO.StreamWriter(s_LessonsPrepLocation))
            {
                string[] titles = data[0].Split(',');
                string title = "";
                for (int i=0; i <= indStudent1; i++)
                {
                    title += (titles[i] + ",");
                }
                for (int i = indStudent10+1; i < titles.Length; i++)
                {
                    title += (titles[i] + ",");
                }
                title += "Duration,FirstInLesson,";
                swPrep.WriteLine(title);

                for (int j = 1; j <= 10; j++)
                {
                    for (int i = 1; i < rows; i++)
                    {
                        string[] fields = data[i].Split(',');
                        Debug.Assert(fields.Length > indStudent10);

                        if (fields[indStudent1 + j - 1].Trim().Length == 0)
                            continue;

                        string strStart = Merge(fields[indDay], fields[indStart], false);
                        string strEnd = Merge(fields[indDay], fields[indEnd], false);
                        DateTime dtStart = DateTime.Parse(strStart);
                        DateTime dtEnd = DateTime.Parse(strEnd);
                        TimeSpan ts = dtEnd - dtStart;
                        double hours = ts.TotalHours;

                        string res = "";
                        for (int k = 0; k < indStudent1; k++)
                            res += (fields[k] + ",");
                        res += fields[indStudent1 + j - 1];
                        res += ",";
                        for (int k = indStudent10 + 1; k < fields.Length; k++)
                            res += (fields[k] + ",");
                        res += hours.ToString();
                        res += (j == 1 ? ",1," : ",0,");
                        swPrep.WriteLine(res);
                    }
                }
            }
        }

        static void PrepPeople(string schemaLoc, string dataLoc, string prepLoc, string fieldName)
        {
            string[] schema = File.ReadAllLines(schemaLoc);
            int indFirstName = FindField(schema, "FirstName", 0);
            Debug.Assert(indFirstName >= 0);
            int indLastName = FindField(schema, "LastName", 0);
            Debug.Assert(indLastName >= 0);

            if (File.Exists(prepLoc))
                File.Delete(prepLoc);

            string[] data = File.ReadAllLines(dataLoc);
            int rows = data.Length;
            Debug.Assert(rows > 1);
            using (System.IO.StreamWriter swPrep = new System.IO.StreamWriter(prepLoc))
            {
                swPrep.WriteLine(data[0] + fieldName + ",");
                for (int i = 1; i < rows; i++)
                {
                    string[] fields = data[i].Split(',');
                    Debug.Assert(fields.Length > indLastName);
                    string tname = Merge(fields[indFirstName], fields[indLastName], true);
                    swPrep.WriteLine(data[i] + tname + ",");
                }
            }
        }

        static string Merge(string fn, string ln, bool quotes)
        {
            string fnc = (fn.StartsWith("\"") ? fn.Substring(1, fn.Length - 2) : fn);
            string lnc = (ln.StartsWith("\"") ? ln.Substring(1, ln.Length - 2) : ln);
            if (quotes)
                return "\"" + fnc + " " + lnc + "\"";
            else
                return fnc + " " + lnc;
        }

        static int FindField(string[] strings, string fieldname, int fieldLocation)
        {
            int ind = -1;
            foreach (string s in strings)
            {
                ind++;
                string[] fields = s.Split(',');
                Debug.Assert(fields.Length > fieldLocation);
                if (fields[fieldLocation] == fieldname)
                    return ind;
            }
            return -1;
        }
    }


}
