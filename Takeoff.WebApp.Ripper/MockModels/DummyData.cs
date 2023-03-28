using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Takeoff.ViewModels;

namespace Takeoff.WebApp.Ripper
{
    static class DummyData
    {
        static DummyData()
        {
            Names =
                @"Angelita Willmore
            Anderson Roseman
            Edwardo Engberg
            Ethelyn Swing
            In Vandenbosch
            Coy Murr
            Shara Luc
            Kai Trevarthen
            Jesusa Greenwell
            Ardelle Dohm
            Margaret Redmond
            Marlana Gold
            Traci Pamplin
            Reita Fontaine
            Demetria Mcconnel
            Machelle Eaton
            Nguyet Moriarty
            Takisha Seabrooks
            Eugene Solley
            Yoshiko Gause
            Mckinley Touchette
            Izetta Archibald
            Nam Culp
            Ninfa Ridder
            Hyon Youmans
            Evangeline Ruddy
            Hilda Antoine
            Verda Criss
            Kittie Kittle
            Roberto Rockett
            Ione Lamprecht
            Elin Parham
            Nathaniel Russum
            Iluminada Michelsen
            Amado Umana
            Tiny Romito
            Dania Sabin
            Luise Lytch
            Rolanda Clausen
            Evie Hennigan
            Coral Birkhead
            Wilfred Burchill
            Judson Olivares
            Nathan Body
            Lonna Kinney
            Tamra Ariza
            Mirella Pirtle
            Janina Votaw
            Galen Wyche
            Verla Shim
            Sadie Ortmann
            Winter Havens
            Priscila Mckissick
            Tomas Okane
            Omega Brodsky
            Carla Caulfield
            Lakendra Rotenberry
            Anjanette Chick
            Mason Veras
            Marielle Dekker
            Michele Nieto
            Zella Godwin
            Stefan Tapia
            Elba Vause
            Myrtice Hedges
            Shirley Lofland
            Diana Canino
            Rey Callen
            Carrie Lisk
            Hayley Mickles
            Claribel Gallien
            Roxy Fye
            Earnest Horowitz
            Deloras Sparacino
            Renato Charters
            Cammie Hauk
            Abbie Claiborne
            Lyman Kash
            Hermine Conger
            Glennie Jobe
            Sanora Raminez
            Rosanne Fitting
            Suanne Carson
            Reinaldo Corral
            Soraya Gilmer
            Shaina Lung
            Nakisha Mccorvey
            Tyree Thomasson
            Kaylee Sayer
            Angel Mcquaid
            Ling Molock
            Frederica Raffaele
            Felecia Spina
            Donovan Castilla
            Victoria Lettinga
            Russ Digirolamo
            Nevada Metoyer
            Ivey Farley
            Gay Anselmo
            Audrie Tennant
            Rey Rafferty
            Rodrick Olson
            Marty Boggess
            Alejandro Rebello
            Loree Keiser
            Gloria Flury
            Darwin Ortego
            Towanda Gossett
            Ethelyn Fravel
            Talia Hiser
            Jarvis Gulyas
            Nakia Benard
            Vincenzo Inabinet
            Britni Celentano
            Calvin Seel
            Mika West
            Vinnie Liberty
            Mitzie Etheridge
            Alejandrina Thorman
            Cordelia Westbrooks
            Cornell Snipes
            Margarett Signor
            Raeann Waldow
            Georgiann Rackham
            Audrie Nellum
            Shaina Alphonse
            Lamar Fava
            Felecia Trask
            Manuel Christofferso
            Bobbie Revelle
            Adela Alverez
            Samira Khang
            Ivory Kraft
            Hyun Harries
            Rodger Lamarr
            Romelia Scipio
            Tiara Million
            Emogene Burchill
            Herma Schlicher
            Sonya Ridings
            Patrice Seegmiller
            Jolanda Kroll
            Anh Klima
            Candie Daggett
            Consuelo Check
            Dania Derouen
            Gladis Ostendorf
            Junita Bonelli
            Sherrell Mauro
            Margeret Mccreery"
                    .SplitLines().Select(n => n.Trim()).Where(s => s.HasChars(CharsThatMatter.NonWhitespace)).ToArray();

            FirstNames = Names.Select(n => n.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).First()).ToArray();
            LastNames = Names.Select(n => n.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Last()).ToArray();

        }

        public static DateTime Date1 = DateTime.Parse("9/1/2011 11:00am").ToUniversalTime();
        public static DateTime Date2 = Date1.AddDays(3).AddMinutes(345).ToUniversalTime();
        public static DateTime Date3 = Date2.AddDays(13).AddMinutes(291).ToUniversalTime();
        public static DateTime Date4 = Date3.AddDays(1).AddMinutes(5).ToUniversalTime();
        public static DateTime Date5 = Date4.AddDays(8).AddMinutes(35).ToUniversalTime();

        public static string[] Names { get; private set; }
        public static string[] FirstNames { get; private set; }
        public static string[] LastNames { get; private set; }

        static Random random = new Random();

        public static string[] Comments =
            @"Nice job censoring!  When we're done remember to give me a version with and without the bleep.
Of course.  I'll include both on DVD for you.
Can you add more color here?
I think a cool texture with a bunch of colors would work well here.
Yeah cool
Is this a good shot here?
Yeah bro it's awesome
We could put some colored texture here or maybe a shot of a bunch of sneaker boxes going all the way to the ceiling.  What do you think?
Sneakers to the ceiling!!!!
Can you switch the background here?  It's a pretty long green screen shot.
Yeah man I'll make it so funky you can smell it.
Can you put a cool background, maybe TV static?
Yeah I got just the thing for that.
You want TV static here also?
Word
Nice job censoring!  When we're done remember to give me a version with and without the bleep.
Of course.  I'll include both on DVD for you.
Can you add more color here?
I think a cool texture with a bunch of colors would work well here.
Yeah cool"
                .SplitLines().Select(n => n.Trim()).Where(s => s.HasChars(CharsThatMatter.NonWhitespace)).ToArray();

        private static string RandomItem(this List<string> list)
        {
            if (list == null || list.Count == 0)
                throw new InvalidOperationException("No items");
            if (list.Count == 1)
                return list[0];
            return list[random.Next(0, list.Count)];
        }


        public static T FillProductionData<T>(this T model) where T : Email_ProductionBase
        {

            model.Thumbnails = new[]
                                   {
                                       new VideoThumbnail
                                           {
                                               Height = 101,
                                               Width = 180,
                                               Url = "rip-assets/videothumb-1.jpg"
                                           },
                                       new VideoThumbnail
                                           {
                                               Height = 101,
                                               Width = 180,
                                               Url = "rip-assets/videothumb-2.jpg"
                                           },
                                       new VideoThumbnail
                                           {
                                               Height = 101,
                                               Width = 180,
                                               Url = "rip-assets/videothumb-3.jpg"
                                           },
                                   };
            model.ProductionId = 45;
            model.ProductionTitle = "NY Video";
            model.ProductionUrl = "Productions_Details.html";
            model.LogoUrl = "rip-assets/nyvideo.png";
            model.LogoHeight = 70;
            model.LogoWidth = 58;
            return model;
        }

    }
}
