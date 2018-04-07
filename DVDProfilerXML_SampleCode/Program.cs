using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using DoenaSoft.DVDProfiler.DVDProfilerHelper;
using Version381 = DoenaSoft.DVDProfiler.DVDProfilerXML.Version381;
using Version400 = DoenaSoft.DVDProfiler.DVDProfilerXML.Version400;

namespace DoenaSoft.DVDProfiler.DVDProfilerXML_SampleCode
{
    public class Program
    {
        [STAThread()]
        public static void Main(String[] args)
        {
            OnlineAccess.Init("Doena Soft.", "DVDProfilerXML_SampleCode");
            OnlineAccess.CheckForNewVersion("http://doena-soft.de/dvdprofiler/3.9.0/versions.xml", null, "DVDProfilerXML", typeof(Program).Assembly);

            if ((args != null) && (args.Length > 0))
            {
                //Special function to check if DVD Profiler XML contains any new elements or attributes that 
                //are not explicitely covered by this schema file or the Serializer class
                //args[0] has to be a collection.xml file
                CheckSchemaValidity(args[0]);

                CreateDVDFromScratch();

                ReadCollectionFile(args[0]);

                ReadDVDFile();

                ReadCastInformationFile();

                ReadCrewInformationFile();

                ReadLocalitiesXOD();

                FilterWithLinq(args[0]);
            }
            else
            {
                CreateDVDFromScratch();

                ReadCollectionFile("Collection.xml");

                ReadDVDFile();

                ReadCastInformationFile();

                ReadCrewInformationFile();

                ReadLocalitiesXOD();

                FilterWithLinq("collection.xml");
            }

            Console.WriteLine("Do you remember the Windows joke about having to press <Start> to shut down?");
            Console.WriteLine("Well, ...");
            Console.WriteLine();
            Console.WriteLine("Press <Enter> to exit.");
            Console.ReadLine();
        }

        private static void CreateDVDFromScratch()
        {
            Version400.User user = new Version400.User();
            user.FirstName = "DJ Doena";

            Version400.Collection collection = new Version400.Collection();
            collection.DVDList = new Version400.DVD[1];
            collection.DVDList[0] = new Version400.DVD();
            collection.DVDList[0].Title = "My Movie";
            collection.DVDList[0].DiscList = new Version400.Disc[1];
            collection.DVDList[0].DiscList[0] = new Version400.Disc();
            collection.DVDList[0].DiscList[0].DescriptionSideA = "Main Feature";
            collection.DVDList[0].AudioList = new Version400.AudioTrack[1];
            collection.DVDList[0].AudioList[0] = new Version400.AudioTrack();
            collection.DVDList[0].AudioList[0].Content = "German";
            collection.DVDList[0].AudioList[0].Format = "Dolby Digital";
            collection.DVDList[0].AudioList[0].Channels = "5.1";
            collection.DVDList[0].SubtitleList = new String[1];
            collection.DVDList[0].SubtitleList[0] = "German";
            collection.DVDList[0].LoanInfo.Loaned = true;
            collection.DVDList[0].LoanInfo.Due = new DateTime(2012, 3, 31);
            collection.DVDList[0].LoanInfo.DueSpecified = true;
            collection.DVDList[0].LoanInfo.User = user;
            collection.DVDList[0].EventList = new Version400.Event[1];
            collection.DVDList[0].EventList[0] = new Version400.Event();
            collection.DVDList[0].EventList[0].Timestamp = DateTime.UtcNow;
            collection.DVDList[0].EventList[0].Type = Version400.EventType.Borrowed;
            collection.DVDList[0].EventList[0].User = user;

            collection.Serialize(@"FromScratch.xml");
        }

        private static void ReadCollectionFile(String inputCollectionFile)
        {
            Version400.Collection collection = DVDProfilerSerializer<Version400.Collection>.Deserialize(inputCollectionFile);

            if ((collection != null) && (collection.DVDList != null) && (collection.DVDList.Length > 0))
            {
                foreach (Version400.DVD dvd in collection.DVDList)
                {
                    Console.WriteLine(dvd.ToString());
                }
            }

            collection.Serialize("Collection_out.xml");

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to continue.");
            Console.ReadLine();

            Version400.Utilities.SortByTitleAscending(collection);

            if ((collection != null) && (collection.DVDList != null) && (collection.DVDList.Length > 0))
            {
                foreach (Version400.DVD dvd in collection.DVDList)
                {
                    Console.WriteLine(dvd.ToString());
                }
            }

            collection.Serialize("Collection_out_sorted.xml");

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to continue.");
            Console.ReadLine();

            Version400.CollectionTree tree = Version400.Utilities.GetCollectionTree(collection);

            Version400.Utilities.SortById(tree, true);

            if (tree.DVDList.Count > 0)
            {
                foreach (Version400.DVDNode dvd in tree.DVDList)
                {
                    WriteHierarchy(dvd, 0);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to continue.");
            Console.ReadLine();

            tree = Version400.Utilities.GetCollectionTree(collection);

            Version400.Utilities.SortByTitleAscending(tree, true);

            if (tree.DVDList.Count > 0)
            {
                foreach (Version400.DVDNode dvd in tree.DVDList)
                {
                    WriteHierarchy(dvd, 0);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to continue.");
            Console.ReadLine();

            tree = Version400.Utilities.GetCollectionTree(collection);

            Version400.Utilities.SortById(tree, true);

            Version400.Utilities.SortByTitleAscending(tree, false);

            if (tree.DVDList.Count > 0)
            {
                foreach (Version400.DVDNode dvd in tree.DVDList)
                {
                    WriteHierarchy(dvd, 0);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to continue.");
            Console.ReadLine();
        }

        private static void ReadDVDFile()
        {
            Version400.DVD dvd = DVDProfilerSerializer<Version400.DVD>.Deserialize("DVD.xml");

            if (dvd != null)
            {
                Console.WriteLine(dvd.Title + " (" + dvd.ID_LocalityDesc + ")");
            }

            dvd.Serialize("DVD_out.xml");

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to continue.");
            Console.ReadLine();
        }

        private static void ReadCastInformationFile()
        {
            Version400.CastInformation cast = DVDProfilerSerializer<Version400.CastInformation>.Deserialize("CastInformation.xml");

            OutputCast(cast);

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to continue.");
            Console.ReadLine();

            Version400.Utilities.CopyCastInformationToClipboard(cast);

            if (Version400.Utilities.TryGetCastInformationFromClipboard(out cast))
            {
                OutputCast(cast);
            }

            cast.Serialize("CastInformation_out.xml");

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to continue.");
            Console.ReadLine();
        }

        private static void WriteHierarchy(Version400.DVDNode node, Int32 indent)
        {
            Console.Write(String.Empty.PadLeft(indent));
            Console.WriteLine(node.ToString());

            if ((node.ChildrenList != null) && (node.ChildrenList.Count > 0))
            {
                foreach (Version400.DVDNode childNode in node.ChildrenList)
                {
                    WriteHierarchy(childNode, indent + 4);
                }
            }
        }

        private static void OutputCast(Version400.CastInformation cast)
        {
            if (cast != null)
            {
                Console.WriteLine(cast.Title);

                if ((cast.CastList != null) && (cast.CastList.Length > 0))
                {
                    foreach (Object item in cast.CastList)
                    {
                        if (item is Version400.Divider)
                        {
                            Version400.Divider divider = (Version400.Divider)item;

                            Console.WriteLine("--- " + divider.ToString() + " ---");
                        }
                        else
                        {
                            Version400.CastMember actor = (Version400.CastMember)item;

                            Console.WriteLine(actor.ToString());
                        }
                    }
                }
            }
        }

        private static void ReadCrewInformationFile()
        {
            Version400.CrewInformation crew = DVDProfilerSerializer<Version400.CrewInformation>.Deserialize("CrewInformation.xml");

            OutputCrew(crew);

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to continue.");
            Console.ReadLine();

            Version400.Utilities.CopyCrewInformationToClipboard(crew);

            if (Version400.Utilities.TryGetCrewInformationFromClipboard(out crew))
            {
                OutputCrew(crew);
            }

            crew.Serialize("CrewInformation_out.xml");

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to continue.");
            Console.ReadLine();
        }

        private static void OutputCrew(Version400.CrewInformation crew)
        {
            if (crew != null)
            {
                Console.WriteLine(crew.Title);

                if ((crew.CrewList != null) && (crew.CrewList.Length > 0))
                {
                    foreach (Object item in crew.CrewList)
                    {
                        if (item is Version400.CrewDivider)
                        {
                            Version400.CrewDivider divider = (Version400.CrewDivider)item;

                            Console.WriteLine("--- " + divider.ToString() + " ---");
                        }
                        else
                        {
                            Version400.CrewMember crewMember = (Version400.CrewMember)item;

                            Console.WriteLine(crewMember.ToString());
                        }
                    }
                }
            }
        }

        private static void ReadLocalitiesXOD()
        {
            Version381.Localities localities = Version381.Localities.Deserialize();

            if ((localities != null) && (localities.LocalityList != null) && (localities.LocalityList.Length > 0))
            {
                List<Version381.Locality> sortedList = new List<Version381.Locality>(localities.LocalityList);

                sortedList.Sort(new Comparison<Version381.Locality>(SortById));

                foreach (Version381.Locality locality in sortedList)
                {
                    Console.WriteLine(locality.ID + ": " + locality.Description);
                }

                Console.WriteLine();
                Console.WriteLine("Press <Enter> to continue.");
                Console.ReadLine();

                sortedList.Sort(new Comparison<Version381.Locality>(SortByName));

                foreach (Version381.Locality locality in sortedList)
                {
                    Console.WriteLine(locality.Description + ": " + locality.ID);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to continue.");
            Console.ReadLine();
        }

        private static Int32 SortById(Version381.Locality left, Version381.Locality right)
        {
            return (left.ID.CompareTo(right.ID));
        }

        private static Int32 SortByName(Version381.Locality left, Version381.Locality right)
        {
            return (left.Description.CompareTo(right.Description));
        }

        private static void CheckSchemaValidity(String arg)
        {
            Version400.Collection collection = DVDProfilerSerializer<Version400.Collection>.Deserialize(arg);

            if ((collection != null) && (collection.DVDList != null) && (collection.DVDList.Length > 0))
            {
                foreach (Version400.DVD dvd in collection.DVDList)
                {
                    String dvdName = dvd.ToString();

                    WriteNodeError(dvd.OpenElements, dvdName + " has open elements:");

                    if ((dvd.AudioList != null) && (dvd.AudioList.Length > 0))
                    {
                        foreach (Version400.AudioTrack track in dvd.AudioList)
                        {
                            WriteNodeError(track.OpenElements, dvdName + " [Audio] has open elements:");
                        }
                    }

                    if (dvd.BoxSet != null)
                    {
                        WriteNodeError(dvd.BoxSet.OpenElements, dvdName + " [BoxSet] has open elements:");
                    }

                    if ((dvd.CastList != null) && (dvd.CastList.Length > 0))
                    {
                        foreach (Object item in dvd.CastList)
                        {
                            Version400.CastMember castMember = item as Version400.CastMember;

                            if (castMember != null)
                            {
                                WriteNodeError(castMember.OpenAttributes, dvdName + " [Actors - Actor] has open attributes:");
                            }
                            else
                            {
                                Version400.Divider divider = (Version400.Divider)item;

                                WriteNodeError(divider.OpenAttributes, dvdName + " [Actors - Divider] has open attributes:");
                            }
                        }
                    }

                    if ((dvd.CrewList != null) && (dvd.CrewList.Length > 0))
                    {
                        foreach (Object item in dvd.CrewList)
                        {
                            Version400.CrewMember crewMember = item as Version400.CrewMember;

                            if (crewMember != null)
                            {
                                WriteNodeError(crewMember.OpenAttributes, dvdName + " [Credits - Credit] has open attributes:");
                            }
                            else
                            {
                                Version400.Divider divider = (Version400.Divider)item;

                                WriteNodeError(divider.OpenAttributes, dvdName + " [Credits - Divider] has open attributes:");
                            }
                        }
                    }

                    if ((dvd.DiscList != null) && (dvd.DiscList.Length > 0))
                    {
                        foreach (Version400.Disc disc in dvd.DiscList)
                        {
                            WriteNodeError(disc.OpenElements, dvdName + " [Disc] has open elements:");
                        }
                    }

                    if ((dvd.EventList != null) && (dvd.EventList.Length > 0))
                    {
                        foreach (Version400.Event eventItem in dvd.EventList)
                        {
                            WriteNodeError(eventItem.OpenElements, dvdName + " [Event] has open elements:");
                        }
                    }

                    if (dvd.Exclusions != null)
                    {
                        WriteNodeError(dvd.Exclusions.OpenElements, dvdName + " [Exclusions] has open elements:");
                    }

                    if (dvd.Features != null)
                    {
                        WriteNodeError(dvd.Features.OpenElements, dvdName + " [Features] has open elements:");
                    }

                    if (dvd.Format != null)
                    {
                        WriteNodeError(dvd.Format.OpenElements, dvdName + " [Format] has open elements:");
                    }

                    if (dvd.LoanInfo != null)
                    {
                        WriteNodeError(dvd.LoanInfo.OpenElements, dvdName + " [LoanInfo] has open elements:");

                        if (dvd.LoanInfo.User != null)
                        {
                            WriteNodeError(dvd.LoanInfo.User.OpenAttributes, dvdName + " [LoanInfo - User] has open attributes:");
                        }
                    }

                    if (dvd.Locks != null)
                    {
                        WriteNodeError(dvd.Locks.OpenElements, dvdName + " [Locks] has open elements:");
                    }

                    if (dvd.MediaBanners != null)
                    {
                        WriteNodeError(dvd.MediaBanners.OpenAttributes, dvdName + " [MediaBanners] has open attributes:");
                    }

                    if (dvd.MediaTypes != null)
                    {
                        WriteNodeError(dvd.MediaTypes.OpenElements, dvdName + " [MediaTypes] has open elements:");
                    }

                    if (dvd.PurchaseInfo != null)
                    {
                        WriteNodeError(dvd.PurchaseInfo.OpenElements, dvdName + " [PurchaseInfo] has open elements:");

                        if (dvd.PurchaseInfo.Price != null)
                        {
                            WriteNodeError(dvd.PurchaseInfo.Price.OpenAttributes, dvdName + " [PurchaseInfo - Price] has open attributes:");
                        }
                    }

                    if (dvd.Review != null)
                    {
                        WriteNodeError(dvd.Review.OpenAttributes, dvdName + " [Review] has open attributes:");
                    }

                    if (dvd.SRP != null)
                    {
                        WriteNodeError(dvd.SRP.OpenAttributes, dvdName + " [SRP] has open attributes:");
                    }

                    if ((dvd.TagList != null) && (dvd.TagList.Length > 0))
                    {
                        foreach (Version400.Tag tag in dvd.TagList)
                        {
                            WriteNodeError(tag.OpenAttributes, dvdName + " [Tag] has open attributes:");
                        }
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to continue.");
            Console.ReadLine();
        }

        private static void WriteNodeError(XmlNode[] nodes, String errorString)
        {
            if ((nodes != null) && (nodes.Length > 0))
            {
                Console.WriteLine(errorString);

                foreach (XmlNode node in nodes)
                {
                    Console.WriteLine(node.OuterXml);
                }
            }
        }

        private static void FilterWithLinq(String inputCollectionFile)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            UsingSerializerClasses(inputCollectionFile);

            GC.Collect();
            GC.WaitForPendingFinalizers();

            UsingRawXml(inputCollectionFile);
        }

        private static void UsingRawXml(String inputCollectionFile)
        {
            //and now a Linq query over the native XML file without the serializer classes, this time we're looking for George Clooney
            XDocument xDoc = XDocument.Load(inputCollectionFile);

            IEnumerable<XElement> allDvds = xDoc.Element("Collection").Elements("DVD");

            String firstName = "George";
            String lastName = "Clooney";

            IEnumerable<XElement> filtered = GetDvdsByActorName(allDvds, firstName, lastName);

            foreach (XElement dvd in filtered)
            {
                Console.WriteLine(dvd.Element("Title").Value);
            }

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to continue.");
            Console.ReadLine();

            //Or a movie with Tom Hanks and Julia Roberts?
            firstName = "Tom";
            lastName = "Hanks";

            filtered = GetDvdsByActorName(allDvds, firstName, lastName);

            firstName = "Julia";
            lastName = "Roberts";

            filtered = GetDvdsByActorName(filtered, firstName, lastName);

            foreach (XElement dvd in filtered)
            {
                Console.WriteLine(dvd.Element("Title").Value);
            }

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to continue.");
            Console.ReadLine();

            //Or what about a movie that starred Tom Hanks or Julia Roberts?
            firstName = "Tom";
            lastName = "Hanks";
            filtered = GetDvdsByActorName(allDvds, firstName, lastName);
            firstName = "Julia";
            lastName = "Roberts";

            IEnumerable<XElement> filtered2 = GetDvdsByActorName(allDvds, firstName, lastName);

            filtered = filtered.Union(filtered2);

            foreach (XElement dvd in filtered)
            {
                Console.WriteLine(dvd.Element("Title").Value);
            }

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to continue.");
            Console.ReadLine();

            //And now we can still create a list of our "normal" DVD objects:
            IEnumerable<Version400.DVD> filteredList = filtered.Select(dvd => DVDProfilerSerializer<Version400.DVD>.FromString(dvd.ToString()));

            foreach (Version400.DVD dvd in filteredList)
            {
                Console.WriteLine(dvd.ToString());
            }

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to continue.");
            Console.ReadLine();

            //Well, if you prefer XPath as your weapon of choice:
            filtered = allDvds.Where(dvd => (Boolean)(dvd.XPathEvaluate("boolean(./Regions/Region[text()='3'])")));

            foreach (XElement dvd in filtered)
            {
                Console.WriteLine(dvd.Element("Title").Value);
            }

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to continue.");
            Console.ReadLine();
        }

        private static IEnumerable<XElement> GetDvdsByActorName(IEnumerable<XElement> dvds, String firstName, String lastName)
        {
            //Ok, What exactly happens here:
            //We have a list of DVD nodes in dvds
            //Now we filter this list under the following conditions
            //1. the DVD node must have an Actors node
            //2. under this Actors node we check if any Actor nodes fullfils the following conditions:
            //2a. the FirstName attribute equals the value of firstName
            //2b. the LastName attribute equals the value of lastName
            IEnumerable<XElement> filtered = dvds.Where(dvd => HasActor(dvd, firstName, lastName));

            return (filtered);
        }

        private static Boolean HasActor(XElement dvd, String firstName, String lastName)
        {
            XElement actors = dvd.Element("Actors");

            if (actors != null)
            {
                IEnumerable<XElement> actorList = actors.Elements("Actor");

                //FirstOrDefault returns the first element that matches the requirements
                XElement actor = actorList.FirstOrDefault(potentialActor => IsActor(potentialActor, firstName, lastName));

                return (actor != null);
            }

            return (false);
        }

        private static Boolean IsActor(XElement actor, String firstName, String lastName)
        {
            XAttribute firstNameAttribute = actor.Attribute("FirstName");

            XAttribute lastNameAttribute = actor.Attribute("LastName");

            return ((firstNameAttribute != null) && (firstNameAttribute.Value == firstName)
                && (lastNameAttribute != null) && (lastNameAttribute.Value == lastName));
        }


        private static void UsingSerializerClasses(String inputCollectionFile)
        {
            Version400.Collection collection = DVDProfilerSerializer<Version400.Collection>.Deserialize(inputCollectionFile);

            if ((collection != null) && (collection.DVDList != null) && (collection.DVDList.Length > 0))
            {
                List<Version400.DVD> list = new List<Version400.DVD>(collection.DVDList);

                //Find all DVDs with Region 1
                //This is a lambda expression:
                IEnumerable<Version400.DVD> filtered = list.Where(dvd => (dvd.RegionList != null && dvd.RegionList.Contains("1")));

                foreach (Version400.DVD dvd in filtered)
                {
                    if (dvd != null)
                    {
                        Console.WriteLine(dvd.Title);
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Press <Enter> to continue.");
                Console.ReadLine();

                //this is the same code using a method:
                filtered = list.Where(new Func<Version400.DVD, Boolean>(FindRegionOne));

                foreach (Version400.DVD dvd in filtered)
                {
                    Console.WriteLine(dvd.Title);
                }

                Console.WriteLine();
                Console.WriteLine("Press <Enter> to continue.");
                Console.ReadLine();

                //Find all DVDs that DON'T have english subs
                filtered = list.Where(dvd => (dvd.SubtitleList != null && dvd.SubtitleList.Contains("English") == false));

                foreach (Version400.DVD dvd in filtered)
                {
                    Console.WriteLine(dvd.Title);
                }

                Console.WriteLine();
                Console.WriteLine("Press <Enter> to continue.");
                Console.ReadLine();
            }
        }

        private static Boolean FindRegionOne(Version400.DVD dvd)
        {
            return (dvd.RegionList != null && dvd.RegionList.Contains("1"));
        }
    }
}