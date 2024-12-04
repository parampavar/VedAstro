using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static VedAstro.Library.HouseName;
//makes accessing Planet and House shorter and sweeter
using static VedAstro.Library.PlanetName;

//█▀▀█ █▀▀▄ █░░ █░░█ 　 █▀▀ █▀▀█ █▀▀▄ █▀▀ 　 █░░░█ █░░█ █▀▀ █▀▀▄ 　 █░░█ █▀▀█ █░░█ 　 █▀▀ █▀▀█ █▀▀▄ █▀▀▄ █▀▀█ ▀▀█▀▀ 
//█░░█ █░░█ █░░ █▄▄█ 　 █░░ █░░█ █░░█ █▀▀ 　 █▄█▄█ █▀▀█ █▀▀ █░░█ 　 █▄▄█ █░░█ █░░█ 　 █░░ █▄▄█ █░░█ █░░█ █░░█ ░░█░░ 
//▀▀▀▀ ▀░░▀ ▀▀▀ ▄▄▄█ 　 ▀▀▀ ▀▀▀▀ ▀▀▀░ ▀▀▀ 　 ░▀░▀░ ▀░░▀ ▀▀▀ ▀░░▀ 　 ▄▄▄█ ▀▀▀▀ ░▀▀▀ 　 ▀▀▀ ▀░░▀ ▀░░▀ ▀░░▀ ▀▀▀▀ ░░▀░░ 

//█▀▄▀█ █▀▀ █▀▀▄ ░▀░ ▀▀█▀▀ █▀▀█ ▀▀█▀▀ █▀▀ ░░ 
//█░▀░█ █▀▀ █░░█ ▀█▀ ░░█░░ █▄▄█ ░░█░░ █▀▀ ▄▄ 
//▀░░░▀ ▀▀▀ ▀▀▀░ ▀▀▀ ░░▀░░ ▀░░▀ ░░▀░░ ▀▀▀ ░█ 

//█▀▀█ █▀▀▄ █░░ █░░█ 　 █▀▄▀█ █▀▀ █▀▀▄ ░▀░ ▀▀█▀▀ █▀▀█ ▀▀█▀▀ █▀▀ 　 █░░░█ █░░█ █▀▀ █▀▀▄ 　 █░░█ █▀▀█ █░░█ 
//█░░█ █░░█ █░░ █▄▄█ 　 █░▀░█ █▀▀ █░░█ ▀█▀ ░░█░░ █▄▄█ ░░█░░ █▀▀ 　 █▄█▄█ █▀▀█ █▀▀ █░░█ 　 █▄▄█ █░░█ █░░█ 
//▀▀▀▀ ▀░░▀ ▀▀▀ ▄▄▄█ 　 ▀░░░▀ ▀▀▀ ▀▀▀░ ▀▀▀ ░░▀░░ ▀░░▀ ░░▀░░ ▀▀▀ 　 ░▀░▀░ ▀░░▀ ▀▀▀ ▀░░▀ 　 ▄▄▄█ ▀▀▀▀ ░▀▀▀ 

//█▀▀ █▀▀█ █▀▀▄ █▀▀▄ █▀▀█ ▀▀█▀▀ 　 █▀▀ █▀▀█ █▀▀▄ █▀▀ 
//█░░ █▄▄█ █░░█ █░░█ █░░█ ░░█░░ 　 █░░ █░░█ █░░█ █▀▀ 
//▀▀▀ ▀░░▀ ▀░░▀ ▀░░▀ ▀▀▀▀ ░░▀░░ 　 ▀▀▀ ▀▀▀▀ ▀▀▀░ ▀▀▀

namespace VedAstro.Library
{
    /// <summary>
    /// Calculators specific for Horoscope events
    /// Only a person's birth time is needed
    /// </summary>
    public class CalculateHoroscope
    {

        //yoga's from BV Raman's Ashtakavarga System Book
        #region ASHTAKAVARGA YOGAS

        //SUN

        /// <summary>
        /// The person will always be ill if the bindus are 3 or 4 and the Sun
        /// does not happen to be in his place of exaltation or own sign.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.SunAshtakavargaYoga2)]
        public static CalculatorResult SunAshtakavargaYoga2(Time birthTime)
        {
            //get bindus for sun
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Sun, birthTime);

            //all below maximum 4
            var correctBindus = bindus <= 4;

            //not happen to be in his place of exaltation or own sign.
            var sunNotExalted = !(Calculate.IsPlanetExaltedSign(Sun, birthTime));
            var sunNotOwnSign = !(Calculate.IsPlanetInOwnSign(Sun, birthTime));

            var isOccuring = correctBindus && sunNotExalted && sunNotOwnSign;

            return CalculatorResult.New(isOccuring, [Sun], birthTime);

        }

        /// <summary>
        /// One will be blessed with all the paraphernalia of political power and
        /// eminence if the Sun as ascendant lord is in a sign having more than 5 or 6 bindus.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.SunAshtakavargaYoga3)]
        public static CalculatorResult SunAshtakavargaYoga3(Time birthTime)
        {
            //get bindus for sun
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Sun, birthTime);

            // having more than 5
            var correctBindus = bindus >= 5;

            //the Sun as ascendant lord
            var sunIsAscendantLord = Calculate.LordOfHouse(House1, birthTime) == Sun;

            var isOccuring = correctBindus && sunIsAscendantLord;

            return CalculatorResult.New(isOccuring, [Sun], birthTime);

        }

        /// <summary>
        /// The native's father may meet with death near a burial ground, 
        /// or a mountain, or a fireplace, or sea if the sign occupied by the Sun 
        /// has more than 5 bindus and happens to be a Kendra or Trikona.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.SunAshtakavargaYoga5)]
        public static CalculatorResult SunAshtakavargaYoga5(Time birthTime)
        {
            //get bindus for sun
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Sun, birthTime);

            // minimum 5 bindus
            var moreThan5 = bindus > 5;

            //happens to be a Kendra or Trikona
            var isKendraOrTrikona = Calculate.IsPlanetInKendra(Sun, birthTime) || Calculate.IsPlanetInTrikona(Sun, birthTime);

            var isOccuring = moreThan5 && isKendraOrTrikona;

            return CalculatorResult.New(isOccuring, [Sun], birthTime);
        }

        /// <summary>
        /// The father expires in the 17th year of the native if the Sun is in a quadrant 
        /// (Kendra) or a trine (Trikona), which should also be a friendly sign associated 
        /// with 3, 4, or 5 bindus.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.SunAshtakavargaYoga6)]
        public static CalculatorResult SunAshtakavargaYoga6(Time birthTime)
        {
            //should also be a friendly sign 
            var isFriendlySign = Calculate.IsPlanetInFriendSign(Sun, birthTime);

            //get bindus for sun
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Sun, birthTime);

            //with 3, 4, or 5 bindus
            var correctBindus = bindus == 3 || bindus == 4 || bindus == 5;

            //happens to be a Kendra or Trikona
            var isKendraOrTrikona = Calculate.IsPlanetInKendra(Sun, birthTime) || Calculate.IsPlanetInTrikona(Sun, birthTime);

            var isOccuring = correctBindus && isFriendlySign && isKendraOrTrikona;

            return CalculatorResult.New(isOccuring, [Sun], birthTime);
        }

        /// <summary>
        /// Death of the father happens after five years if the Sun is in the 9th with Rahu, 
        /// and the number of bindus is 3.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.SunAshtakavargaYoga7)]
        public static CalculatorResult SunAshtakavargaYoga7(Time birthTime)
        {
            // Sun is in the 9th
            var sunIn9th = Calculate.IsPlanetInHouse(Sun, House9, birthTime);

            // Sun with Rahu
            var sunWithRahu = Calculate.IsPlanetInHouse(Rahu, House9, birthTime);

            //get bindus for sun
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Sun, birthTime);

            //number of bindus is maximum 3
            var correctBindus = bindus <= 3;

            var isOccuring = correctBindus && sunIn9th && sunWithRahu;

            return CalculatorResult.New(isOccuring, [Sun, Rahu], birthTime);

        }

        /// <summary>
        /// The Sun in the 5th having 3 bindus, in association with Moon, Mars, and Saturn, 
        /// destroys parents and brothers.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.SunAshtakavargaYoga8)]
        public static CalculatorResult SunAshtakavargaYoga8(Time birthTime)
        {
            // Sun in the 5th
            var sunIn5th = Calculate.IsPlanetInHouse(Sun, House5, birthTime);

            //get bindus for sun
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Sun, birthTime);

            //number of bindus is maximum 3
            var correctBindus = bindus <= 3;

            // association with Moon, Mars, and Saturn
            var moonIn5th = Calculate.IsPlanetInHouse(Moon, House5, birthTime);
            var marsIn5th = Calculate.IsPlanetInHouse(Mars, House5, birthTime);
            var saturnIn5th = Calculate.IsPlanetInHouse(Saturn, House5, birthTime);
            var association = moonIn5th && marsIn5th && saturnIn5th;

            var isOccuring = correctBindus && sunIn5th && association;

            return CalculatorResult.New(isOccuring, [Sun, Moon, Mars, Saturn], birthTime);
        }

        /// <summary>
        /// Death of the father takes place after the 20th year if the Sun is in the 3rd 
        /// with 3 or 4 bindus aspected by malefics from the 9th.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.SunAshtakavargaYoga9)]
        public static CalculatorResult SunAshtakavargaYoga9(Time birthTime)
        {
            // Sun in the 3rd
            var sunIn3rd = Calculate.IsPlanetInHouse(Sun, House3, birthTime);

            //get bindus for sun
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Sun, birthTime);

            //number of bindus maximum 4
            var correctBindus = bindus <= 4;

            //get all malefics aspecting sun, then check if any are in 9th house
            var maleficsAspectingSun = Calculate.GetAllMaleficPlanetsAspecting(Sun, birthTime);
            var maleficsAspectingFrom9th = Calculate.IsAnyPlanetsInHouse(maleficsAspectingSun, House9, birthTime);

            var isOccuring = sunIn3rd && correctBindus && maleficsAspectingFrom9th;

            return CalculatorResult.New(isOccuring, [House9], [Sun], birthTime);

        }

        /// <summary>
        /// When the Sun is in the 6th in a Rasi having 4 bindus, and aspected by Jupiter, 
        /// the father dies after the 25th year.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.SunAshtakavargaYoga10)]
        public static CalculatorResult SunAshtakavargaYoga10(Time birthTime)
        {
            // Sun in the 6th
            var sunIn6 = Calculate.IsPlanetInHouse(Sun, House6, birthTime);

            // get bindus for sun
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Sun, birthTime);

            // number of bindus is maximum 6
            // TODO NOTE: discrepancy in book, text on pg 38 says 4 bindus
            //            but tested example given on pg 39 says 6 bindus
            var correctBindus = bindus <= 6;

            // aspected by Jupiter
            var aspectedByJupiter = Calculate.IsPlanetAspectedByPlanet(Sun, Jupiter, birthTime);

            var isOccuring = sunIn6 && correctBindus && aspectedByJupiter;

            return CalculatorResult.New(isOccuring, [House6], [Sun, Jupiter], birthTime);

        }

        /// <summary>
        /// Death of the father will happen between the 30th and 36th year of the native if the 
        /// Sun is in a trine or angle and has five or more bindus.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.SunAshtakavargaYoga11)]
        public static CalculatorResult SunAshtakavargaYoga11(Time birthTime)
        {
            // Sun in Trikona or angle (quadrant)
            var isKendraOrTrikona = Calculate.IsPlanetInKendra(Sun, birthTime) || Calculate.IsPlanetInTrikona(Sun, birthTime);

            // get bindus for sun
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Sun, birthTime);

            // has five or more bindus
            var correctBindus = bindus >= 5;

            var isOccuring = isKendraOrTrikona && correctBindus;

            return CalculatorResult.New(isOccuring, [Sun], birthTime);

        }

        //MOON

        /// <summary>
        /// If the Rasi occupied by the Moon is 
        /// associated with 1,2,3 bindus (in his own Ashtakavarga),
        /// the person suffers from distress, diseases, litigation and ill-health.
        /// In addition to this, if two or three planets join the Moon,
        /// the native is likely to pass away in his 37th year.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MoonAshtakavargaYoga1A)]
        public static CalculatorResult MoonAshtakavargaYoga1A(Time birthTime)
        {
            //get bindus for moon
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Moon, birthTime);

            //associated with maximum 3 bindus
            var isOccuring = bindus <= 3;

            return CalculatorResult.New(isOccuring, [Moon], birthTime);
        }

        /// <summary>
        /// If the Rasi occupied by the Moon is 
        /// associated with 1,2,3 bindus (in his own Ashtakavarga),
        /// the person suffers from distress, diseases, litigation and ill-health.
        /// In addition to this, if two or three planets join the Moon,
        /// the native is likely to pass away in his 37th year.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MoonAshtakavargaYoga1B)]
        public static CalculatorResult MoonAshtakavargaYoga1B(Time birthTime)
        {
            //If the Rasi occupied by the Moon is associated with 1,2,3 bindus (in his own Ashtakavarga)
            var yoga1A = MoonAshtakavargaYoga1A(birthTime).Occuring;

            //two or three planets join the Moon,
            var moonSign = Calculate.PlanetZodiacSign(Moon, birthTime);
            //all planets in same sign as moon
            var planetsWithMoon = Calculate.PlanetsInSign(moonSign.GetSignName(), birthTime);
            //do not count moon
            planetsWithMoon.Remove(Moon);
            var moonWith2Or3Planets = planetsWithMoon.Count is 2 or 3;

            var isOccuring = yoga1A && moonWith2Or3Planets;

            return CalculatorResult.New(isOccuring, [Moon], birthTime);
        }

        /// <summary>
        /// The Moon in the 6th or 11th associated with 
        /// more than 6 bindus is found to make one rich 
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MoonAshtakavargaYoga2B)]
        public static CalculatorResult MoonAshtakavargaYoga2B(Time birthTime)
        {
            //Moon in the 6th or 11th 
            var moonHouse = Calculate.HousePlanetOccupiesBasedOnLongitudes(Moon, birthTime);
            var isMoonIn6th11th = moonHouse == House6 || moonHouse == House11;

            //associated with more than 6 bindus
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Moon, birthTime);
            var moreThan6 = bindus >= 6;

            //both condition meet
            var isOccuring = isMoonIn6th11th && moreThan6;

            return CalculatorResult.New(isOccuring, [Moon], birthTime);
        }

        /// <summary>
        /// One born when the Moon is in a Kendra and associated with 7 to 8 bindus,
        /// becomes highly learned, powerful, a ruler and endowed with wealth.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MoonAshtakavargaYoga4)]
        public static CalculatorResult MoonAshtakavargaYoga4(Time birthTime)
        {
            // Moon is in a Kendra
            var isMoonInKendra = Calculate.IsPlanetInKendra(Moon, birthTime);

            // associated minimum 7
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Moon, birthTime);
            var correctBindus = bindus >= 7;

            // both conditions meet
            var isOccuring = isMoonInKendra && correctBindus;

            return CalculatorResult.New(isOccuring, [Moon], birthTime);

        }

        /// <summary>
        /// The mother suffers from frequent ill-health
        /// if Rahu is in the 2nd and the Moon occupies the 7th or 8th with 1, 2 or 3 bindus
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MoonAshtakavargaYoga6)]
        public static CalculatorResult MoonAshtakavargaYoga6(Time birthTime)
        {
            // Rahu is in the 2nd
            var rahuIn2nd = Calculate.IsPlanetInHouse(Rahu, House2, birthTime);

            // The Moon occupies the 7th or 8th
            var moonHouse = Calculate.HousePlanetOccupiesBasedOnLongitudes(Moon, birthTime);
            var moonIn7thOr8th = moonHouse == House7 || moonHouse == House8;

            // with maximum 3 bindus
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Moon, birthTime);
            var correctBindus = bindus <= 3;

            // all conditions meet
            var isOccuring = rahuIn2nd && moonIn7thOr8th && correctBindus;

            return CalculatorResult.New(isOccuring, [Moon, Rahu], birthTime);
        }

        /// <summary>
        /// If Mars associated with less bindus is in the 4th or 8th from the Moon, then separation from mother takes place.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MoonAshtakavargaYoga7)]
        public static CalculatorResult MoonAshtakavargaYoga7(Time birthTime)
        {
            // Determine the sign position of Mars relative to the Moon
            var marsHouseFromMoon = Calculate.SignDistanceFromPlanetToPlanet(Moon, Mars, birthTime);

            // Check if Mars is in the 4th or 8th house from the Moon
            var marsIn4thOr8thFromMoon = marsHouseFromMoon == 4 || marsHouseFromMoon == 8;

            // Get bindus for Mars (in moon's ashtakvarga)
            var marsBindus = Calculate.PlanetAshtakvargaBinduByPlanet(Moon, Mars, birthTime);

            // Check if Mars has less bindus (assuming "less" means below a specific threshold, e.g., 3)
            var marsHasLessBindus = marsBindus <= 3;

            // Check both conditions
            var isOccuring = marsIn4thOr8thFromMoon && marsHasLessBindus;

            return CalculatorResult.New(isOccuring, [Mars, Moon], birthTime);

        }

        /// <summary>
        /// Mother's death is likely in the 36th year if the Moon occupies a kendra or
        /// the 12th with 1 to 3 bindus and the 4th house is joined by malefic planets.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MoonAshtakavargaYoga8)]
        public static CalculatorResult MoonAshtakavargaYoga8(Time birthTime)
        {
            // Moon occupies a Kendra (4th, 7th, or 10th house)
            var moonInKendra = Calculate.IsPlanetInKendra(Moon, birthTime);

            // Moon occupies the 12th house
            var moonIn12th = Calculate.IsPlanetInHouse(Moon, House12, birthTime);

            // Check if Moon is in Kendra or 12th house
            var moonInKendraOr12th = moonInKendra || moonIn12th;

            // Moon has maximum 3 bindus in its own Ashtakavarga
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Moon, birthTime);
            var correctBindus = bindus <= 3;

            // Check if 4th house is joined by malefic planets
            var maleficIn4th = Calculate.IsMaleficPlanetInHouse(House4, birthTime);

            // All conditions must be met
            var isOccuring = moonInKendraOr12th && correctBindus && maleficIn4th;

            // Return the calculation result
            return CalculatorResult.New(isOccuring, [House4], [Moon], birthTime);

        }

        /// <summary>
        /// The waning Moon in Lagna associated with 1, 2, 3 or no bindus indicates
        /// danger from weapons and poisonous insects; there is also a likelihood of the mother's death.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MoonAshtakavargaYoga9)]
        public static CalculatorResult MoonAshtakavargaYoga9(Time birthTime)
        {
            // Check if the Moon is waning
            var isMoonWaning = Calculate.IsWaningMoon(birthTime);

            // Check if Moon is in Lagna (House1)
            var moonInLagna = Calculate.IsPlanetInHouse(Moon, House1, birthTime);

            // Get bindus for Moon
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Moon, birthTime);

            // Check if bindus maximum 3
            var correctBindus = bindus <= 3;

            // All conditions must be met
            var isOccuring = isMoonWaning && moonInLagna && correctBindus;

            return CalculatorResult.New(isOccuring, [Moon], birthTime);

        }

        /// <summary>
        /// Mother's death can take place in the 36th year if the 4th house is occupied by malefics and
        /// the Moon is in Lagna associated with few bindus.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MoonAshtakavargaYoga10)]
        public static CalculatorResult MoonAshtakavargaYoga10(Time birthTime)
        {
            // Check if the 4th house is occupied by malefics
            var maleficIn4th = Calculate.IsMaleficPlanetInHouse(House4, birthTime);

            // Check if Moon is in Lagna (House1)
            var moonInLagna = Calculate.IsPlanetInHouse(Moon, House1, birthTime);

            // Get bindus for Moon in its own Ashtakavarga
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Moon, birthTime);

            // Check if bindus are few (e.g., 1 to 3)
            var bindusAreFew = bindus <= 3;

            // All conditions must be met
            var isOccuring = maleficIn4th && moonInLagna && bindusAreFew;

            // Return the calculation result
            return CalculatorResult.New(isOccuring, [House4], [Moon], birthTime);
        }

        //MARS
        /// <summary>
        /// When Mars is exalted or is in his own sign occupying the 9th, 4th, 10th or 1st house,
        /// associated with 8 bindus, the person becomes a millionaire.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga2)]
        public static CalculatorResult MarsAshtakavargaYoga2(Time birthTime)
        {
            // Check if Mars is exalted
            var marsIsExalted = Calculate.IsPlanetExaltedSign(Mars, birthTime);

            // Check if Mars is in his own sign
            var marsInOwnSign = Calculate.IsPlanetInOwnSign(Mars, birthTime);

            // Mars is exalted or in his own sign
            var marsExaltedOrOwnSign = marsIsExalted || marsInOwnSign;

            // Check if Mars is occupying the 9th, 4th, 10th, or 1st house
            var marsIn1st = Calculate.IsPlanetInHouse(Mars, House1, birthTime);
            var marsIn4th = Calculate.IsPlanetInHouse(Mars, House4, birthTime);
            var marsIn9th = Calculate.IsPlanetInHouse(Mars, House9, birthTime);
            var marsIn10th = Calculate.IsPlanetInHouse(Mars, House10, birthTime);

            // Mars is in one of the specified houses
            var marsInCorrectHouse = marsIn1st || marsIn4th || marsIn9th || marsIn10th;

            // Get bindus for Mars in his own Ashtakavarga
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);

            // Check if Mars is associated with 8 bindus
            var correctBindus = bindus >= 8;

            // All conditions must be met
            var isOccurring = marsExaltedOrOwnSign && marsInCorrectHouse && correctBindus;

            // Return the result
            return CalculatorResult.New(isOccurring, [Mars], birthTime);
        }

        /// <summary>
        /// One will become a ruler if Sagittarius, Leo, Aries, Cancer, or Capricorn
        /// happens to be the ascendant and Mars occupies it, associated with 4 bindus.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga3)]
        public static CalculatorResult MarsAshtakavargaYoga3(Time birthTime)
        {
            // Defining the required ascendant signs
            var requiredAscendantSigns = new[]
            {
                ZodiacName.Sagittarius,
                ZodiacName.Leo,
                ZodiacName.Aries,
                ZodiacName.Cancer,
                ZodiacName.Capricorn
            };

            // Get the name of the ascendant sign
            var ascendantSign = Calculate.LagnaSignName(birthTime);

            // Check if the ascendant is one of the required signs
            var isAscendantInRequiredSigns = requiredAscendantSigns.Contains(ascendantSign);

            // Check if Mars occupies the ascendant (House1)
            var marsInAscendant = Calculate.IsPlanetInHouse(Mars, House1, birthTime);

            // Get bindus for Mars in his own Ashtakavarga
            var marsBindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);

            // Check if Mars is associated with minimum 4 bindus
            var marsHasFourBindus = marsBindus >= 4;

            // All conditions must be met
            var isOccurring = isAscendantInRequiredSigns && marsInAscendant && marsHasFourBindus;

            // Return the result
            return CalculatorResult.New(isOccurring, [Mars], birthTime);
        }

        /// <summary>
        /// Generally, if Mars has 8 bindus one rises to a high position such as head of a small principality.
        /// In modern parlance, it means a high government official or even a minister, depending upon the strength of Mars.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga4)]
        public static CalculatorResult MarsAshtakavargaYoga4(Time birthTime)
        {
            // Get bindus for Mars in his own Ashtakavarga
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);

            // Check if Mars has 8 bindus
            var hasEightBindus = bindus == 8;

            var isOccurring = hasEightBindus;

            // Return the result
            return CalculatorResult.New(isOccurring, [Mars], birthTime);
        }

        /// <summary>
        /// If in addition to acquiring 8 bindus, Mars happens to be in the ascendant
        /// or the 10th or the 2nd, he will become a king or head of a government.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga5)]
        public static CalculatorResult MarsAshtakavargaYoga5(Time birthTime)
        {
            // Get bindus for Mars in his own Ashtakavarga
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);

            // Check if Mars is associated with 8 bindus
            var correctBindus = bindus == 8;

            // Check if Mars is occupying the ascendant (House1), 2nd, or 10th house
            var marsInAscendant = Calculate.IsPlanetInHouse(Mars, House1, birthTime);
            var marsIn2nd = Calculate.IsPlanetInHouse(Mars, House2, birthTime);
            var marsIn10th = Calculate.IsPlanetInHouse(Mars, House10, birthTime);

            var marsInCorrectHouse = marsInAscendant || marsIn2nd || marsIn10th;

            // All conditions must be met
            var isOccurring = correctBindus && marsInCorrectHouse;

            // Return the result
            return CalculatorResult.New(isOccurring, [Mars], birthTime);
        }

        /// <summary>
        /// "in addition to these" = "If Mars has 8 bindus and Mars happens to be in the ascendant or the 10th or the 2nd"
        /// Original Text : If in addition to these if Mars is also exalted or in his own house, the person becomes a powerful ruler.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga6)]
        public static CalculatorResult MarsAshtakavargaYoga6(Time birthTime)
        {
            // Get the number of bindus for Mars in its own Ashtakavarga
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);
            var hasEightBindus = bindus == 8;

            // Check if Mars is in the ascendant (1st house), 10th, or 2nd house
            var marsInAscendant = Calculate.IsPlanetInHouse(Mars, House1, birthTime);
            var marsIn10th = Calculate.IsPlanetInHouse(Mars, House10, birthTime);
            var marsIn2nd = Calculate.IsPlanetInHouse(Mars, House2, birthTime);
            var marsInCorrectHouse = marsInAscendant || marsIn10th || marsIn2nd;

            // Check if Mars is exalted or in his own sign
            var marsIsExalted = Calculate.IsPlanetExaltedSign(Mars, birthTime);
            var marsInOwnSign = Calculate.IsPlanetInOwnSign(Mars, birthTime);
            var marsExaltedOrOwnSign = marsIsExalted || marsInOwnSign;

            // All conditions must be met
            var isOccurring = hasEightBindus && marsInCorrectHouse && marsExaltedOrOwnSign;

            // Return the result
            return CalculatorResult.New(isOccurring, [Mars], birthTime);
        }

        /// <summary>
        /// One will become wealthy if Mars, associated with 4 or more bindus, 
        /// occupies the Lagna, Chandra Lagna (Moon's house), or the 9th or 10th from Moon, 
        /// which should also happen to be his own or exaltation sign.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga7)]
        public static CalculatorResult MarsAshtakavargaYoga7(Time birthTime)
        {
            // Get bindus for Mars in his own Ashtakavarga
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);

            // Check if Mars has 4 or more bindus
            var correctBindus = bindus >= 4;

            // Check if Mars occupies Lagna (1st house)
            var marsInLagna = Calculate.IsPlanetInHouse(Mars, House1, birthTime);

            //NOTE: discrepancy in book text pg52 vs test text pg 53 (using test text instead)
            // Check if Mars occupies the 9th house from Moon
            var marsIn9thFromMoon = Calculate.SignDistanceFromPlanetToPlanet(Moon, Mars, birthTime) == 9;

            // Check if Mars occupies the 10th house from Moon
            var marsIn10thFromMoon = Calculate.SignDistanceFromPlanetToPlanet(Moon, Mars, birthTime) == 10;

            // Get the house occupied by the Moon (Chandra Lagna)
            var moonHouse = Calculate.HousePlanetOccupiesBasedOnLongitudes(Moon, birthTime);

            // Check if Mars occupies the same house as Moon (Chandra Lagna)
            var marsInChandraLagna = Calculate.IsPlanetInHouse(Mars, moonHouse, birthTime);

            // Mars is in one of the specified houses
            var marsInCorrectHouse = marsInLagna || marsIn9thFromMoon || marsIn10thFromMoon || marsInChandraLagna;

            // Check if Mars is in his own sign
            var marsInOwnSign = Calculate.IsPlanetInOwnSign(Mars, birthTime);

            // Check if Mars is in his exaltation sign
            var marsIsExalted = Calculate.IsPlanetExaltedSign(Mars, birthTime);

            // The sign occupied by Mars should be his own sign or exaltation sign
            var marsInOwnOrExaltationSign = marsInOwnSign || marsIsExalted;

            // All conditions must be met
            var isOccurring = correctBindus && marsInCorrectHouse && marsInOwnOrExaltationSign;

            // Return the result
            return CalculatorResult.New(isOccurring, [Mars], birthTime);
        }

        /// <summary>
        /// Mars as lord of the 2nd in the 6th endowed with 6 bindus
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga8)]
        public static CalculatorResult MarsAshtakavargaYoga8(Time birthTime)
        {
            // Mars as lord of the 2nd
            var lordOf2ndHouse = Calculate.LordOfHouse(House2, birthTime);
            bool marsIsLordOf2nd = lordOf2ndHouse == Mars;

            // Mars in the 6th
            bool marsIn6th = Calculate.IsPlanetInHouse(Mars, House6, birthTime);

            // Mars endowed with 6 bindus
            int marsBindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);
            bool marsHas6Bindus = marsBindus == 6;

            // All conditions met
            bool isOccuring = marsIsLordOf2nd && marsIn6th && marsHas6Bindus;

            return CalculatorResult.New(isOccuring, [Mars], birthTime);
        }

        /// <summary>
        /// Mars as lord of Lagna or the 8th, is in Lagna, or Chandra Lagna or in the 10th or 9th,
        /// associated with no or a few bindus and joined by a debilitated or inimical planet.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga9)]
        public static CalculatorResult MarsAshtakavargaYoga9(Time birthTime)
        {
            // Condition A: Mars as lord of Lagna or the 8th
            var marsIsLordOfLagna = Calculate.LordOfHouse(House1, birthTime) == Mars;
            var marsIsLordOf8th = Calculate.LordOfHouse(House8, birthTime) == Mars;
            var marsIsLordOfLagnaOr8th = marsIsLordOfLagna || marsIsLordOf8th;

            // Condition B: Mars is in Lagna, or Chandra Lagna, or in the 10th or 9th
            var marsInLagna = Calculate.IsPlanetInHouse(Mars, House1, birthTime);
            var marsSign = Calculate.PlanetZodiacSign(Mars, birthTime).GetSignName();
            var moonSign = Calculate.PlanetZodiacSign(Moon, birthTime).GetSignName();
            var marsInChandraLagna = marsSign == moonSign;
            var marsIn9th = Calculate.IsPlanetInHouse(Mars, House9, birthTime);
            var marsIn10th = Calculate.IsPlanetInHouse(Mars, House10, birthTime);
            var marsIsInCorrectPlace = marsInLagna || marsInChandraLagna || marsIn9th || marsIn10th;

            // Condition C: Mars has no or few bindus (0 to 3)
            var marsBindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);
            var marsHasFewBindus = marsBindus <= 3;

            // Condition D: Mars is joined by a debilitated or inimical planet (malefic planet)
            var planetsWithMars = Calculate.PlanetsInSign(marsSign, birthTime);
            planetsWithMars.RemoveAll(p => p == Mars); // Exclude Mars

            var inimicalPlanetsToMars = Calculate.MaleficPlanetList(birthTime);

            bool hasDebilitatedOrInimicalPlanetConjunctMars = false;
            var relevantPlanets = new List<PlanetName> { Mars };
            foreach (var planet in planetsWithMars)
            {
                var isPlanetDebilitated = Calculate.IsPlanetDebilitated(planet, birthTime);
                var isInimicalToMars = inimicalPlanetsToMars.Contains(planet);
                if (isPlanetDebilitated || isInimicalToMars)
                {
                    hasDebilitatedOrInimicalPlanetConjunctMars = true;
                    relevantPlanets.Add(planet);
                }
            }

            // Final Condition: All conditions must be met
            var isOccurring = marsIsLordOfLagnaOr8th && marsIsInCorrectPlace && marsHasFewBindus && hasDebilitatedOrInimicalPlanetConjunctMars;

            return CalculatorResult.New(isOccurring, relevantPlanets.ToArray(), birthTime);
        }

        /// <summary>
        /// Mars with 6 bindus occupies the 6th, 8th or 12th being debilitated or combust and joined by waxing Moon
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga10A)]
        public static CalculatorResult MarsAshtakavargaYoga10A(Time birthTime)
        {
            // Get bindus for Mars in his own Ashtakavarga
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);

            // Check if Mars has max 6 bindus
            var marsHas6Bindus = bindus <= 6;

            // Check if Mars occupies the 6th, 8th, or 12th house
            var marsIn6th = Calculate.IsPlanetInHouse(Mars, House6, birthTime);
            var marsIn8th = Calculate.IsPlanetInHouse(Mars, House8, birthTime);
            var marsIn12th = Calculate.IsPlanetInHouse(Mars, House12, birthTime);
            var marsIn6th8th12th = marsIn6th || marsIn8th || marsIn12th;

            // Check if Mars is debilitated
            var marsIsDebilitated = Calculate.IsPlanetDebilitated(Mars, birthTime);

            // Check if Mars is combust
            var marsIsCombust = Calculate.IsPlanetCombust(Mars, birthTime);

            // Mars is debilitated or combust
            var marsIsDebilitatedOrCombust = marsIsDebilitated || marsIsCombust;

            // Check if Mars is joined by waxing Moon
            // First, check if Mars and Moon are in the same sign (conjunct)
            var marsConjunctMoon = Calculate.IsPlanetConjunctWithPlanet(Mars, Moon, birthTime);

            // Check if Moon is waxing
            var isMoonWaxing = Calculate.IsWaxingMoon(birthTime);

            // So, Mars is joined by waxing Moon
            var marsJoinedByWaxingMoon = marsConjunctMoon && isMoonWaxing;

            // All conditions must be met
            var isOccurring = marsHas6Bindus && marsIn6th8th12th && marsIsDebilitatedOrCombust && marsJoinedByWaxingMoon;

            // Return the result
            return CalculatorResult.New(isOccurring, [Mars, Moon], birthTime);
        }

        /// <summary>
        /// Mars with 6 bindus occupies the 6th, 8th or 12th being debilitated or combust and joined by waxing Moon and Mars is strong
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga10B)]
        public static CalculatorResult MarsAshtakavargaYoga10B(Time birthTime)
        {
            // Step 1: Mars with minimum 6 bindus
            int marsBindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);
            bool marsHas6Bindus = marsBindus >= 6;

            // Step 2: Mars occupies the 6th, 8th, or 12th house
            bool marsIn6th = Calculate.IsPlanetInHouse(Mars, House6, birthTime);
            bool marsIn8th = Calculate.IsPlanetInHouse(Mars, House8, birthTime);
            bool marsIn12th = Calculate.IsPlanetInHouse(Mars, House12, birthTime);
            bool marsIn6th8th12th = marsIn6th || marsIn8th || marsIn12th;

            // Step 3: Mars is debilitated or combust
            bool marsDebilitated = Calculate.IsPlanetDebilitated(Mars, birthTime);
            bool marsCombust = Calculate.IsPlanetCombust(Mars, birthTime);
            bool marsDebilitatedOrCombust = marsDebilitated || marsCombust;

            // Step 4: Mars is joined by waxing Moon
            bool moonConjunctMars = Calculate.IsPlanetConjunctWithPlanet(Mars, Moon, birthTime);
            bool moonIsWaxing = !Calculate.IsWaxingMoon(birthTime);
            bool marsWithWaxingMoon = moonConjunctMars && moonIsWaxing;

            // Step 5: Mars is strong
            bool marsIsStrong = Calculate.IsPlanetStrongInShadbala(Mars, birthTime);

            // All conditions must be met
            bool isOccurring = marsHas6Bindus && marsIn6th8th12th && marsDebilitatedOrCombust && marsWithWaxingMoon && marsIsStrong;

            // Return the result
            return CalculatorResult.New(isOccurring, [Mars, Moon], birthTime);
        }

        /// <summary>
        /// Mars, possessing 4 bindus, occupies the 5th, Lagna, or a quadrant
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga11)]
        public static CalculatorResult MarsAshtakavargaYoga11(Time birthTime)
        {
            // Get bindus for Mars in his own Ashtakavarga
            var bindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);

            // Check if Mars has minimum 4 bindus
            var has4Bindus = bindus >= 4;

            // Check if Mars occupies the 5th house
            var marsIn5th = Calculate.IsPlanetInHouse(Mars, House5, birthTime);

            // Check if Mars occupies Lagna (1st house)
            var marsInLagna = Calculate.IsPlanetInHouse(Mars, House1, birthTime);

            // Check if Mars occupies any quadrant (Kendra)
            var marsInQuadrant = Calculate.IsPlanetInKendra(Mars, birthTime);

            // Mars occupies the 5th, Lagna, or a quadrant
            var marsInCorrectHouse = marsIn5th || marsInLagna || marsInQuadrant;

            // All conditions must be met
            var isOccurring = has4Bindus && marsInCorrectHouse;

            // Return the result
            return CalculatorResult.New(isOccurring, [Mars], birthTime);
        }

        /// <summary>
        /// Condition 1: If Mars or Saturn with 1 to 3 bindus occupy the 3rd house.
        /// Condition 2: If the lord of the 3rd or the lord of the sign occupied by Karaka (Mars) is in an even sign.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga12A)]
        public static CalculatorResult MarsAshtakavargaYoga12A(Time birthTime)
        {
            // Condition 1:

            // Check Mars
            var marsBindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);
            var marsHas1to3Bindus = marsBindus <= 3; //max 3
            var marsIn3rdHouse = Calculate.IsPlanetInHouse(Mars, House3, birthTime);
            var marsCondition = marsIn3rdHouse && marsHas1to3Bindus;

            // Check Saturn (in Mars's Ashtakvarga )
            var saturnBindus = Calculate.PlanetAshtakvargaBinduByPlanet(Mars, Saturn, birthTime);
            var saturnHas1to3Bindus = saturnBindus <= 3; //max 3
            var saturnIn3rdHouse = Calculate.IsPlanetInHouse(Saturn, House3, birthTime);
            var saturnCondition = saturnIn3rdHouse && saturnHas1to3Bindus;

            var condition1 = marsCondition || saturnCondition;

            // Condition 2:

            // Lord of the 3rd house
            var lordOf3rd = Calculate.LordOfHouse(House3, birthTime);

            // Check if the lord of the 3rd is in an even sign
            var lordOf3rdSign = Calculate.PlanetZodiacSign(lordOf3rd, birthTime).GetSignName();
            var lordOf3rdInEvenSign = Calculate.IsEvenSign(lordOf3rdSign);

            // Lord of the sign occupied by Karaka (Mars)
            var marsSign = Calculate.PlanetZodiacSign(Mars, birthTime).GetSignName();
            var lordOfMarsSign = Calculate.LordOfZodiacSign(marsSign);

            // Check if the lord of Mars's sign is in an even sign
            var lordOfMarsSignSign = Calculate.PlanetZodiacSign(lordOfMarsSign, birthTime).GetSignName();
            var lordOfMarsSignInEvenSign = Calculate.IsEvenSign(lordOfMarsSignSign);

            var condition2 = lordOf3rdInEvenSign || lordOfMarsSignInEvenSign;

            // Both conditions must be met
            var isOccurring = condition1 && condition2;

            return CalculatorResult.New(isOccurring, [House3], [Mars, Saturn], birthTime);
        }

        /// <summary>
        /// Condition 1: If Mars or Saturn with 1 to 3 bindus occupy the 3rd house.
        /// Condition 2: If the lord of the 3rd or the lord of the sign occupied by Karaka (Mars) is in an odd sign.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga12B)]
        public static CalculatorResult MarsAshtakavargaYoga12B(Time birthTime)
        {
            // Condition 1:

            // Check Mars
            var marsBindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);
            var marsHas1to3Bindus = marsBindus <= 3; // max 3
            var marsIn3rdHouse = Calculate.IsPlanetInHouse(Mars, House3, birthTime);
            var marsCondition = marsIn3rdHouse && marsHas1to3Bindus;

            // Check Saturn
            var saturnBindus = Calculate.PlanetAshtakvargaBinduByPlanet(Mars, Saturn, birthTime);
            var saturnHas1to3Bindus = saturnBindus <= 3; // max 3
            var saturnIn3rdHouse = Calculate.IsPlanetInHouse(Saturn, House3, birthTime);
            var saturnCondition = saturnIn3rdHouse && saturnHas1to3Bindus;

            var condition1 = marsCondition || saturnCondition;

            // Condition 2:

            // Lord of the 3rd house
            var lordOf3rd = Calculate.LordOfHouse(House3, birthTime);

            // Check if the lord of the 3rd is in an odd sign
            var lordOf3rdSign = Calculate.PlanetZodiacSign(lordOf3rd, birthTime).GetSignName();
            var lordOf3rdInOddSign = Calculate.IsOddSign(lordOf3rdSign);

            // Lord of the sign occupied by Karaka (Mars)
            var marsSign = Calculate.PlanetZodiacSign(Mars, birthTime).GetSignName();
            var lordOfMarsSign = Calculate.LordOfZodiacSign(marsSign);

            // Check if the lord of Mars's sign is in an odd sign
            var lordOfMarsSignSign = Calculate.PlanetZodiacSign(lordOfMarsSign, birthTime).GetSignName();
            var lordOfMarsSignInOddSign = Calculate.IsOddSign(lordOfMarsSignSign);

            var condition2 = lordOf3rdInOddSign || lordOfMarsSignInOddSign;

            // Both conditions must be met
            var isOccurring = condition1 && condition2;

            return CalculatorResult.New(isOccurring, [House3], [Mars, Saturn], birthTime);
        }

        /// <summary>
        /// If the ascendant is a movable sign, and Saturn with 1 to 3 bindus occupies a common sign
        /// OR If the ascendant is a common sign and Saturn is in a movable sign
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga13A)]
        public static CalculatorResult MarsAshtakavargaYoga13A(Time birthTime)
        {
            // Saturn with 1 to 3 bindus (note assumed that both conditions need 1-3 bindus)
            var saturnBindus = Calculate.PlanetAshtakvargaBinduByPlanet(Mars, Saturn, birthTime);
            var saturnHas1To3Bindus = saturnBindus <= 3; // max 3

            // Get the ascendant sign
            var ascendantSign = Calculate.LagnaSignName(birthTime);

            // Get Saturn's sign
            var saturnSign = Calculate.PlanetZodiacSign(Saturn, birthTime).GetSignName();

            // Option A:
            // If the ascendant is a movable sign
            var ascendantIsMovable = Calculate.IsMovableSign(ascendantSign);

            // Saturn occupies a common sign
            var saturnInCommonSign = Calculate.IsCommonSign(saturnSign);

            var optionA = ascendantIsMovable && saturnInCommonSign && saturnHas1To3Bindus;

            // Option B:
            // If the ascendant is a common sign
            var ascendantIsCommon = Calculate.IsCommonSign(ascendantSign);

            // Saturn is in a movable sign
            var saturnInMovableSign = Calculate.IsMovableSign(saturnSign);

            var optionB = ascendantIsCommon && saturnInMovableSign && saturnHas1To3Bindus;

            // Determine if the yoga is occurring
            var isOccurring = optionA || optionB;

            // Return the result
            return CalculatorResult.New(isOccurring, [Mars, Saturn], birthTime);
        }

        /// <summary>
        /// If the ascendant is a fixed sign and Saturn or Mars is associated with 1 to 3 bindus
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga13B)]
        public static CalculatorResult MarsAshtakavargaYoga13B(Time birthTime)
        {
            // Get the ascendant sign
            var ascendantSign = Calculate.LagnaSignName(birthTime);

            // Check if the ascendant is a fixed sign
            var ascendantIsFixedSign = Calculate.IsFixedSign(ascendantSign);

            // Get bindus for Saturn in its own Ashtakavarga
            var saturnBindus = Calculate.PlanetAshtakvargaBinduByPlanet(Mars, Saturn, birthTime);

            // Check if Saturn has max 3 bindus
            var saturnHas1to3Bindus = saturnBindus <= 3;

            // Get bindus for Mars in its own Ashtakavarga
            var marsBindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);

            // Check if Mars has max 3 bindus
            var marsHas1to3Bindus = marsBindus <= 3;

            // Check if either Saturn or Mars has 1 to 3 bindus
            var saturnOrMarsHas1to3Bindus = saturnHas1to3Bindus || marsHas1to3Bindus;

            // Determine if the yoga is occurring
            var isOccurring = ascendantIsFixedSign && saturnOrMarsHas1to3Bindus;

            // Return the calculation result
            return CalculatorResult.New(isOccurring, [Saturn, Mars], birthTime);
        }

        /// <summary>
        /// The ascendant being a movable sign and Saturn with 5 bindus is in a movable or fixed sign
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga14)]
        public static CalculatorResult MarsAshtakavargaYoga14(Time birthTime)
        {
            // Condition A: The ascendant being a movable sign
            var ascendantSign = Calculate.LagnaSignName(birthTime);
            var ascendantIsMovable = Calculate.IsMovableSign(ascendantSign);

            // Condition B: Saturn with minimum 5 bindus
            var saturnBindus = Calculate.PlanetAshtakvargaBinduByPlanet(Mars, Saturn, birthTime);
            var saturnHas5Bindus = saturnBindus >= 5;

            // Condition C: Saturn is in a movable or fixed sign
            var saturnSign = Calculate.PlanetZodiacSign(Saturn, birthTime).GetSignName();
            var saturnInMovableSign = Calculate.IsMovableSign(saturnSign);
            var saturnInFixedSign = Calculate.IsFixedSign(saturnSign);
            var saturnInMovableOrFixedSign = saturnInMovableSign || saturnInFixedSign;

            // All conditions must be met
            var isOccurring = ascendantIsMovable && saturnHas5Bindus && saturnInMovableOrFixedSign;

            // Return the result
            return CalculatorResult.New(isOccurring, [Saturn, Mars], birthTime);
        }

        /// <summary>
        /// When the Lagna is a fixed sign and the 3rd lord is in a common sign with 5 bindus
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga15)]
        public static CalculatorResult MarsAshtakavargaYoga15(Time birthTime)
        {
            // When the Lagna is a fixed sign
            var lagnaSign = Calculate.LagnaSignName(birthTime);
            var isLagnaFixed = Calculate.IsFixedSign(lagnaSign);

            // The lord of the 3rd house
            var lordOf3rd = Calculate.LordOfHouse(House3, birthTime);

            // The 3rd lord is in a common sign
            var lordOf3rdSign = Calculate.PlanetZodiacSign(lordOf3rd, birthTime).GetSignName();
            var isLordOf3rdInCommonSign = Calculate.IsCommonSign(lordOf3rdSign);

            // The 3rd lord has minimum 5 bindus in Mars Ashtakavarga
            var bindus = Calculate.PlanetAshtakvargaBinduByPlanet(Mars, lordOf3rd, birthTime);
            var has5Bindus = bindus >= 5;

            // All conditions must be met
            var isOccurring = isLagnaFixed && isLordOf3rdInCommonSign && has5Bindus;

            // Return the result
            return CalculatorResult.New(isOccurring, [lordOf3rd], birthTime);
        }

        /// <summary>
        /// Mars is in the 3rd with 5 or more bindus associated with or aspected by a benefic
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga16)]
        public static CalculatorResult MarsAshtakavargaYoga16(Time birthTime)
        {
            // Mars is in the 3rd
            var marsIn3rd = Calculate.IsPlanetInHouse(Mars, House3, birthTime);

            // Mars has 5 or more bindus
            var marsBindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);
            var marsHas5OrMoreBindus = marsBindus >= 5;

            // Check if Mars is conjunct with any benefic planet
            var isMarsConjunctBenefic = Calculate.IsPlanetConjunctWithBeneficPlanets(Mars, birthTime);

            // Check if any benefic planet aspects Mars
            var isMarsAspectedByBenefic = Calculate.IsPlanetAspectedByBeneficPlanets(Mars, birthTime);

            // Mars is associated with or aspected by a benefic
            var marsAssociatedOrAspectedByBenefic = isMarsConjunctBenefic || isMarsAspectedByBenefic;

            // All conditions must be met
            var isOccurring = marsIn3rd && marsHas5OrMoreBindus && marsAssociatedOrAspectedByBenefic;

            // Return the result
            return CalculatorResult.New(isOccurring, [Mars], birthTime);
        }

        /// <summary>
        /// There will be loss of brothers when:
        /// (a) Mars, devoid of bindus and joined by the lord of the 7th, is debilitated or combust or is in association with malefics, 
        /// (b) when Mars and Saturn occupy Lagna, associated with 1 to 3 bindus, 
        /// (c) when Mars endowed with 1 to 3 bindus is in the 8th from Saturn or vice versa, 
        /// (d) when Mars is in the 8th from Saturn and malefics are in the 3rd from Lagna, both with 1 to 3 bindus.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga17)]
        public static CalculatorResult MarsAshtakavargaYoga17(Time birthTime)
        {
            bool isOccurring = false;

            // (a) Mars, devoid of bindus and joined by the lord of the 7th, is debilitated or combust or is in association with malefics

            // Mars, devoid of bindus
            int marsBindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);
            bool marsNoBindus = marsBindus == 0;

            // Mars is joined by the lord of the 7th
            PlanetName lordOf7th = Calculate.LordOfHouse(House7, birthTime);
            bool marsJoinedLordOf7th = Calculate.IsPlanetConjunctWithPlanet(Mars, lordOf7th, birthTime);

            // Mars is debilitated or combust
            bool marsDebilitated = Calculate.IsPlanetDebilitated(Mars, birthTime);
            bool marsCombust = Calculate.IsPlanetCombust(Mars, birthTime);
            bool marsDebilitatedOrCombust = marsDebilitated || marsCombust;

            // Mars is in association with malefics
            bool marsAssociatedWithMalefics = Calculate.IsPlanetConjunctWithMaleficPlanets(Mars, birthTime);

            // Check condition (a)
            bool conditionA = marsNoBindus && marsJoinedLordOf7th && (marsDebilitatedOrCombust || marsAssociatedWithMalefics);

            if (conditionA)
            {
                isOccurring = true;
                goto End;
            }

            // (b) Mars and Saturn occupy Lagna, associated with 1 to 3 bindus

            // Mars and Saturn occupy Lagna
            bool marsInLagna = Calculate.IsPlanetInHouse(Mars, House1, birthTime);
            bool saturnInLagna = Calculate.IsPlanetInHouse(Saturn, House1, birthTime);

            // Mars and Saturn have max 3 bindus
            bool marsHas1to3Bindus = marsBindus <= 3;
            int saturnBindus = Calculate.PlanetAshtakvargaBinduByPlanet(Mars, Saturn, birthTime);
            bool saturnHas1to3Bindus = saturnBindus <= 3;

            bool conditionB = marsInLagna && saturnInLagna && marsHas1to3Bindus && saturnHas1to3Bindus;

            if (conditionB)
            {
                isOccurring = true;
                goto End;
            }

            // (c) when Mars endowed with 1 to 3 bindus is in the 8th from Saturn or vice versa

            // Calculate distances
            int marsFromSaturn = Calculate.SignDistanceFromPlanetToPlanet(Saturn, Mars, birthTime);
            int saturnFromMars = Calculate.SignDistanceFromPlanetToPlanet(Mars, Saturn, birthTime);

            // Check if Mars (max 3 bindus) is in the 8th from Saturn or vice versa
            bool marsIn8thFromSaturn = marsFromSaturn == 8 && marsHas1to3Bindus;
            bool saturnIn8thFromMars = saturnFromMars == 8 && saturnHas1to3Bindus;

            bool conditionC = marsIn8thFromSaturn || saturnIn8thFromMars;

            if (conditionC)
            {
                isOccurring = true;
                goto End;
            }

            // (d) when Mars is in the 8th from Saturn and malefics are in the 3rd from Lagna, both with 1 to 3 bindus

            // Mars is in the 8th from Saturn
            bool marsIn8thFromSaturn_D = marsFromSaturn == 8;

            // Malefics in the 3rd from Lagna
            List<PlanetName> maleficsIn3rd = Calculate.PlanetsInHouseBasedOnSign(House3, birthTime)
                .Where(p => Calculate.IsPlanetMalefic(p, birthTime)).ToList();

            bool maleficsIn3rdFromLagna = maleficsIn3rd.Any();

            // Mars has max 3 bindus
            bool marsHas1to3Bindus_D = marsBindus <= 3;

            // Malefics in 3rd have max 3 bindus (in Mars Ashtakvarga)
            bool maleficsIn3rdWith1to3Bindus = maleficsIn3rd.Any(malefic =>
            {
                int bindus = Calculate.PlanetAshtakvargaBinduByPlanet(Mars, malefic, birthTime);
                return bindus <= 3;
            });

            bool conditionD = marsIn8thFromSaturn_D && marsHas1to3Bindus_D && maleficsIn3rdFromLagna && maleficsIn3rdWith1to3Bindus;

            if (conditionD)
            {
                isOccurring = true;
                goto End;
            }

        End:
            // Return the result
            return CalculatorResult.New(isOccurring, [Mars, Saturn], birthTime);
        }

        /// <summary>
        /// If Mars is in the 3rd house associated with three bindus
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga19)]
        public static CalculatorResult MarsAshtakavargaYoga19(Time birthTime)
        {
            // Check if Mars is in the 3rd house
            var marsIn3rdHouse = Calculate.IsPlanetInHouse(Mars, House3, birthTime);

            // Get the bindus for Mars in its own Ashtakavarga
            var marsBindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);

            // Check if Mars has 3 bindus
            var marsHas3Bindus = marsBindus == 3;

            // Both conditions must be met
            var isOccurring = marsIn3rdHouse && marsHas3Bindus;

            return CalculatorResult.New(isOccurring, [House3], [Mars], birthTime);
        }

        /// <summary>
        /// If Mars occupies Kendra or Trikona identical with Sagittarius, Aries, or Capricorn associated with 4 bindus
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga22)]
        public static CalculatorResult MarsAshtakavargaYoga22(Time birthTime)
        {
            // Get the sign Mars is in
            var marsSign = Calculate.PlanetZodiacSign(Mars, birthTime).GetSignName();

            // Check if Mars is in Sagittarius, Aries, or Capricorn
            var requiredSigns = new[] { ZodiacName.Sagittarius, ZodiacName.Aries, ZodiacName.Capricorn };
            var marsInRequiredSign = requiredSigns.Contains(marsSign);

            // Check if Mars is in Kendra or Trikona
            var marsInKendraOrTrikona = Calculate.IsPlanetInKendra(Mars, birthTime) || Calculate.IsPlanetInTrikona(Mars, birthTime);

            // Get bindus for Mars in his own Ashtakavarga
            var marsBindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);

            // Check if Mars has minimum 4 bindus
            var marsHas4Bindus = marsBindus >= 4;

            // All conditions must be met
            var isOccurring = marsInRequiredSign && marsInKendraOrTrikona && marsHas4Bindus;

            // Return the result
            return CalculatorResult.New(isOccurring, [Mars], birthTime);
        }

        /// <summary>
        /// If Mars with 5 or more bindus is in conjunction with or in mutual aspect with Saturn
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga23)]
        public static CalculatorResult MarsAshtakavargaYoga23(Time birthTime)
        {
            // Get bindus for Mars in its own Ashtakavarga
            int marsBindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);

            // Check if Mars has 5 or more bindus
            bool marsHas5OrMoreBindus = marsBindus >= 5;

            // Check if Mars is in conjunction with Saturn
            bool marsConjunctSaturn = Calculate.IsPlanetConjunctWithPlanet(Mars, Saturn, birthTime);

            // Check if Mars is aspected by Saturn
            bool marsAspectedBySaturn = Calculate.IsPlanetAspectedByPlanet(Mars, Saturn, birthTime);

            // Check if Saturn is aspected by Mars
            bool saturnAspectedByMars = Calculate.IsPlanetAspectedByPlanet(Saturn, Mars, birthTime);

            // Check if Mars is in mutual aspect with Saturn
            bool marsInMutualAspectWithSaturn = marsAspectedBySaturn && saturnAspectedByMars;

            // If Mars is conjunct or in mutual aspect with Saturn
            bool marsConjunctOrMutualAspectSaturn = marsConjunctSaturn || marsInMutualAspectWithSaturn;

            // All conditions must be met
            bool isOccurring = marsHas5OrMoreBindus && marsConjunctOrMutualAspectSaturn;

            // Return the result
            return CalculatorResult.New(isOccurring, [Mars, Saturn], birthTime);
        }

        /// <summary>
        /// If Mars with 1 to 3 bindus is in conjunction with or aspected by Mercury
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga24)]
        public static CalculatorResult MarsAshtakavargaYoga24(Time birthTime)
        {
            // Get bindus for Mars in his own Ashtakavarga
            int marsBindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);

            // Check if Mars has max 3 bindus
            bool marsHas1to3Bindus = marsBindus <= 3;

            // Check if Mars is in conjunction with Mercury
            bool marsConjunctMercury = Calculate.IsPlanetConjunctWithPlanet(Mars, Mercury, birthTime);

            // Check if Mercury aspects Mars
            bool marsAspectedByMercury = Calculate.IsPlanetAspectedByPlanet(Mars, Mercury, birthTime);

            // So, Mars is with Mercury (either conjunct or aspected)
            bool marsWithMercury = marsConjunctMercury || marsAspectedByMercury;

            // All conditions must be met
            bool isOccurring = marsHas1to3Bindus && marsWithMercury;

            // Return the result
            return CalculatorResult.New(isOccurring, [Mars, Mercury], birthTime);
        }

        /// <summary>
        /// If Mars with 5 or more bindus joins or is aspected by the Moon
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MarsAshtakavargaYoga25)]
        public static CalculatorResult MarsAshtakavargaYoga25(Time birthTime)
        {
            // Get Mars bindus in its own Ashtakavarga
            var marsBindus = Calculate.PlanetOwnAshtakvargaBindu(Mars, birthTime);

            // Check if Mars has 5 or more bindus
            var marsHas5OrMoreBindus = marsBindus >= 5;

            // Check if Mars joins Moon (conjunct)
            var marsConjunctMoon = Calculate.IsPlanetConjunctWithPlanet(Mars, Moon, birthTime);

            // Check if Mars is aspected by Moon
            var marsAspectedByMoon = Calculate.IsPlanetAspectedByPlanet(Mars, Moon, birthTime);

            // Mars joins or is aspected by Moon
            var marsJoinedOrAspectedByMoon = marsConjunctMoon || marsAspectedByMoon;

            // All conditions must be met
            var isOccurring = marsHas5OrMoreBindus && marsJoinedOrAspectedByMoon;

            // Return the result
            return CalculatorResult.New(isOccurring, [Mars, Moon], birthTime);
        }

        //MERCURY

        /// <summary>
        /// When Mercury with 1 to 3 bindus is in the 6th or 8th devoid of benefic aspects
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MercuryAshtakavargaYoga3)]
        public static CalculatorResult MercuryAshtakavargaYoga3(Time birthTime)
        {
            // Get bindus for Mercury in its own Ashtakavarga
            int mercuryBindus = Calculate.PlanetOwnAshtakvargaBindu(Mercury, birthTime);

            // Check if Mercury has maximum 3 bindus
            bool mercuryHas1to3Bindus = mercuryBindus <= 3;

            // Check if Mercury is in the 6th or 8th house
            bool mercuryIn6th = Calculate.IsPlanetInHouse(Mercury, House6, birthTime);
            bool mercuryIn8th = Calculate.IsPlanetInHouse(Mercury, House8, birthTime);
            bool mercuryIn6thOr8th = mercuryIn6th || mercuryIn8th;

            // Check if Mercury is devoid of benefic aspects
            bool mercuryAspectedByBenefics = Calculate.IsPlanetAspectedByBeneficPlanets(Mercury, birthTime);
            bool mercuryDevoidOfBeneficAspects = !mercuryAspectedByBenefics;

            // All conditions must be met
            bool isOccurring = mercuryHas1to3Bindus && mercuryIn6thOr8th && mercuryDevoidOfBeneficAspects;

            // Return the result
            return CalculatorResult.New(isOccurring, [House6, House8], [Mercury], birthTime);
        }

        /// <summary>
        /// Associated with 1 to 3 bindus, if Mercury occupies the 6th, 8th or 12th in conjunction with Venus
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MercuryAshtakavargaYoga4)]
        public static CalculatorResult MercuryAshtakavargaYoga4(Time birthTime)
        {
            // Get bindus for Mercury in its own Ashtakavarga
            var mercuryBindus = Calculate.PlanetOwnAshtakvargaBindu(Mercury, birthTime);

            // Check if Mercury has maximum 3 bindus
            var mercuryHas1To3Bindus = mercuryBindus <= 3;

            // Check if Mercury occupies the 6th, 8th, or 12th house
            var mercuryIn6th = Calculate.IsPlanetInHouse(Mercury, House6, birthTime);
            var mercuryIn8th = Calculate.IsPlanetInHouse(Mercury, House8, birthTime);
            var mercuryIn12th = Calculate.IsPlanetInHouse(Mercury, House12, birthTime);

            var mercuryIn6th8th12th = mercuryIn6th || mercuryIn8th || mercuryIn12th;

            // Check if Mercury is in conjunction with Venus
            var mercuryConjunctVenus = Calculate.IsPlanetConjunctWithPlanet(Mercury, Venus, birthTime);

            // All conditions must be met
            var isOccurring = mercuryHas1To3Bindus && mercuryIn6th8th12th && mercuryConjunctVenus;

            // Return the result
            return CalculatorResult.New(isOccurring, [House6, House8, House12], [Mercury, Venus], birthTime);
        }

        /// <summary>
        /// When Mercury is in a trine or a quadrant associated with 5 bindus, and joined or aspected by Jupiter or Saturn
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MercuryAshtakavargaYoga5)]
        public static CalculatorResult MercuryAshtakavargaYoga5(Time birthTime)
        {
            // Check if Mercury is in a trine or a quadrant (kendra)
            var mercuryInTrine = Calculate.IsPlanetInTrikona(Mercury, birthTime);
            var mercuryInQuadrant = Calculate.IsPlanetInKendra(Mercury, birthTime);
            var mercuryInTrineOrQuadrant = mercuryInTrine || mercuryInQuadrant;

            // Get bindus for Mercury in its own Ashtakavarga
            var mercuryBindus = Calculate.PlanetOwnAshtakvargaBindu(Mercury, birthTime);

            // Check if Mercury has minimum 5 bindus
            var mercuryHas5Bindus = mercuryBindus >= 5;

            // Check if Mercury is joined or aspected by Jupiter or Saturn

            // Check if Mercury is conjunct (joined) with Jupiter or Saturn
            var mercuryConjunctJupiter = Calculate.IsPlanetConjunctWithPlanet(Mercury, Jupiter, birthTime);
            var mercuryConjunctSaturn = Calculate.IsPlanetConjunctWithPlanet(Mercury, Saturn, birthTime);
            var mercuryJoinedWithJupiterOrSaturn = mercuryConjunctJupiter || mercuryConjunctSaturn;

            // Check if Mercury is aspected by Jupiter or Saturn
            var mercuryAspectedByJupiter = Calculate.IsPlanetAspectedByPlanet(Mercury, Jupiter, birthTime);
            var mercuryAspectedBySaturn = Calculate.IsPlanetAspectedByPlanet(Mercury, Saturn, birthTime);
            var mercuryAspectedByJupiterOrSaturn = mercuryAspectedByJupiter || mercuryAspectedBySaturn;

            var mercuryJoinedOrAspectedByJupiterOrSaturn = mercuryJoinedWithJupiterOrSaturn || mercuryAspectedByJupiterOrSaturn;

            // All conditions must be met
            var isOccurring = mercuryInTrineOrQuadrant && (mercuryHas5Bindus) && mercuryJoinedOrAspectedByJupiterOrSaturn;

            // Return the result, including the relevant planets
            return CalculatorResult.New(isOccurring, [Mercury, Jupiter, Saturn], birthTime);
        }

        /// <summary>
        /// If Mercury with 5 bindus is in the 4th or 6th from Saturn and Jupiter occupies or aspects the 2nd
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MercuryAshtakavargaYoga6)]
        public static CalculatorResult MercuryAshtakavargaYoga6(Time birthTime)
        {
            // Mercury with minimum 5 bindus
            int mercuryBindus = Calculate.PlanetOwnAshtakvargaBindu(Mercury, birthTime);
            bool mercuryHas5Bindus = mercuryBindus >= 5;

            // Mercury is in the 4th or 6th from Saturn
            int mercuryFromSaturn = Calculate.SignDistanceFromPlanetToPlanet(Saturn, Mercury, birthTime);
            bool mercuryIn4thOr6thFromSaturn = mercuryFromSaturn == 4 || mercuryFromSaturn == 6;

            // Jupiter occupies or aspects the 2nd house
            bool jupiterIn2nd = Calculate.IsPlanetInHouse(Jupiter, House2, birthTime);
            bool jupiterAspects2nd = Calculate.IsHouseAspectedByPlanet(House2, Jupiter, birthTime);

            bool jupiterOccupiesOrAspects2nd = jupiterIn2nd || jupiterAspects2nd;

            // All conditions must be met
            bool isOccurring = mercuryHas5Bindus && mercuryIn4thOr6thFromSaturn && jupiterOccupiesOrAspects2nd;

            // Return the result
            return CalculatorResult.New(isOccurring, [House2], [Mercury, Saturn, Jupiter], birthTime);
        }

        /// <summary>
        /// When Mercury, with 5 bindus, conjoins Jupiter or is in association with or aspected by Mars
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MercuryAshtakavargaYoga7)]
        public static CalculatorResult MercuryAshtakavargaYoga7(Time birthTime)
        {
            // Get bindus for Mercury in its own Ashtakavarga
            int mercuryBindus = Calculate.PlanetOwnAshtakvargaBindu(Mercury, birthTime);

            // Check if Mercury has minimum 5 bindus
            bool mercuryHas5Bindus = mercuryBindus >= 5;

            // Check if Mercury conjoins Jupiter
            bool mercuryConjunctJupiter = Calculate.IsPlanetConjunctWithPlanet(Mercury, Jupiter, birthTime);

            // Check if Mercury is in association with Mars (conjunction)
            bool mercuryConjunctMars = Calculate.IsPlanetConjunctWithPlanet(Mercury, Mars, birthTime);

            // Check if Mercury is aspected by Mars
            bool mercuryAspectedByMars = Calculate.IsPlanetAspectedByPlanet(Mercury, Mars, birthTime);

            // Mercury is in association with or aspected by Mars
            bool mercuryWithMars = mercuryConjunctMars || mercuryAspectedByMars;

            // The yoga occurs when Mercury has 5 bindus and
            // (Mercury conjoins Jupiter OR Mercury is associated with or aspected by Mars)
            bool isOccurring = mercuryHas5Bindus && (mercuryConjunctJupiter || mercuryWithMars);

            // Collect relevant planets
            List<PlanetName> relevantPlanets = [Mercury];
            if (mercuryConjunctJupiter)
            {
                relevantPlanets.Add(Jupiter);
            }
            if (mercuryWithMars)
            {
                relevantPlanets.Add(Mars);
            }

            // Return the result
            return CalculatorResult.New(isOccurring, relevantPlanets.ToArray(), birthTime);
        }

        /// <summary>
        /// If Ketu, associated with minimum 3 bindus (in Mercury’s Ashtakavarga), is in the 5th house or with the lord of the 5th house
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MercuryAshtakavargaYoga8)]
        public static CalculatorResult MercuryAshtakavargaYoga8(Time birthTime)
        {
            // Get the number of bindus that Ketu has in Mercury's Ashtakavarga
            int ketuBindusInMercuryAshtakavarga = Calculate.PlanetAshtakvargaBinduByPlanet(Mercury, Ketu, birthTime);

            // Check if Ketu has minimum 3 bindus in Mercury's Ashtakavarga
            bool ketuHasThreeBindus = ketuBindusInMercuryAshtakavarga >= 3;

            // Check if Ketu is in the 5th house
            bool ketuInFifthHouse = Calculate.IsPlanetInHouse(Ketu, House5, birthTime);

            // Get the lord of the 5th house
            var lordOfFifthHouse = Calculate.LordOfHouse(House5, birthTime);

            // Check if Ketu is with the lord of the 5th house
            bool ketuConjunctWithLordOfFifth = Calculate.IsPlanetConjunctWithPlanet(Ketu, lordOfFifthHouse, birthTime);

            // The condition is met if Ketu has 3 bindus in Mercury's Ashtakavarga and
            // Ketu is in the 5th house or with the lord of the 5th house
            bool isOccurring = ketuHasThreeBindus && (ketuInFifthHouse || ketuConjunctWithLordOfFifth);

            // Return the result
            return CalculatorResult.New(isOccurring, [Mercury, Ketu], birthTime);
        }

        /// <summary>
        /// Disposition of Mercury with 4 bindus in a sign of Mars and in a Navamsa of Venus, aspected by Jupiter
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MercuryAshtakavargaYoga9)]
        public static CalculatorResult MercuryAshtakavargaYoga9(Time birthTime)
        {
            // Get bindus for Mercury in its own Ashtakavarga
            int mercuryBindus = Calculate.PlanetOwnAshtakvargaBindu(Mercury, birthTime);

            // Check if Mercury has minimum 4 bindus
            bool mercuryHas4Bindus = mercuryBindus >= 4;

            // Get Mercury's sign
            var mercurySign = Calculate.PlanetZodiacSign(Mercury, birthTime).GetSignName();

            // Get the lord of Mercury's sign
            var mercurySignLord = Calculate.LordOfZodiacSign(mercurySign);

            // Check if the lord is Mars
            bool mercuryInSignOfMars = mercurySignLord == Mars;

            // Get Mercury's Navamsa sign
            var mercuryNavamsaSign = Calculate.PlanetNavamshaD9Sign(Mercury, birthTime).GetSignName();

            // Get the lord of Mercury's Navamsa sign
            var mercuryNavamsaLord = Calculate.LordOfZodiacSign(mercuryNavamsaSign);

            // Check if Navamsa lord is Venus
            bool mercuryInNavamsaOfVenus = mercuryNavamsaLord == Venus;

            // Check if Mercury is aspected by Jupiter
            bool mercuryAspectedByJupiter = Calculate.IsPlanetAspectedByPlanet(Mercury, Jupiter, birthTime);

            // All conditions must be met
            bool isOccurring = mercuryHas4Bindus && mercuryInSignOfMars && mercuryInNavamsaOfVenus && mercuryAspectedByJupiter;

            // Return the result
            return CalculatorResult.New(isOccurring, [Mercury, Jupiter], birthTime);
        }

        /// <summary>
        /// if the lord of the sign occupied by Mercury happens to be placed in a trine or quadrant associated with more bindus
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MercuryAshtakavargaYoga12A)]
        public static CalculatorResult MercuryAshtakavargaYoga12A(Time birthTime)
        {
            // Get the sign occupied by Mercury
            var mercurySign = Calculate.PlanetZodiacSign(Mercury, birthTime).GetSignName();

            // Get the lord of Mercury's sign
            var mercurySignLord = Calculate.LordOfZodiacSign(mercurySign);

            // Check if the lord of Mercury's sign is placed in a trine or quadrant (kendra)
            bool lordInTrine = Calculate.IsPlanetInTrikona(mercurySignLord, birthTime);
            bool lordInQuadrant = Calculate.IsPlanetInKendra(mercurySignLord, birthTime);
            bool isLordInTrineOrQuadrant = lordInTrine || lordInQuadrant;

            // Get bindus for the lord of Mercury's sign in its own Ashtakavarga
            int lordBindus = Calculate.PlanetAshtakvargaBinduByPlanet(Mercury, Venus, birthTime);

            // Determine if the lord is associated with more bindus (assuming 5 or more is "more bindus")
            bool hasMoreBindus = lordBindus >= 5;

            // All conditions must be met
            bool isOccurring = isLordInTrineOrQuadrant && hasMoreBindus;

            // Return the result
            return CalculatorResult.New(isOccurring, [Mercury, mercurySignLord], birthTime);
        }

        /// <summary>
        /// If the lord of the sign occupied by Mercury happens to be placed in the 6th, 8th, or 12th house
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.MercuryAshtakavargaYoga12B)]
        public static CalculatorResult MercuryAshtakavargaYoga12B(Time birthTime)
        {
            // Get the sign occupied by Mercury
            var mercurySign = Calculate.PlanetZodiacSign(Mercury, birthTime).GetSignName();

            // Get the lord of the sign occupied by Mercury
            var lordOfMercurySign = Calculate.LordOfZodiacSign(mercurySign);

            // Determine the house in which the lord of Mercury's sign is placed
            var houseOfLord = Calculate.HousePlanetOccupiesBasedOnLongitudes(lordOfMercurySign, birthTime);

            // Check if the lord is in the 6th, 8th, or 12th house
            bool isLordIn6th = houseOfLord == House6;
            bool isLordIn8th = houseOfLord == House8;
            bool isLordIn12th = houseOfLord == House12;

            var isOccurring = isLordIn6th || isLordIn8th || isLordIn12th;

            // Return the result, including the relevant houses and the planet involved
            return CalculatorResult.New(isOccurring, [House6, House8, House12], [lordOfMercurySign, Mercury], birthTime);
        }


        #endregion

        #region 300 IMPORTANT YOGAS (RAMAN)

        /// <summary>
        /// Definition.-If Jupiter is in a kendra from the
        /// Moon the combination goes under the name Gajakesari.
        /// 
        /// Results.-Many relations, polite and generous,
        /// builder of villages and towns or magistrate over
        /// them; will have a lasting reputation even long after
        /// death.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.GajakesariYoga)]
        public static CalculatorResult GajakesariYoga(Time birthTime)
        {
            //If Jupiter is in a kendra from the Moon
            var jupiterInKendraFromMoon = Calculate.IsPlanetInKendraFromPlanet(Jupiter, Moon, birthTime);

            return CalculatorResult.New(jupiterInKendraFromMoon, [Jupiter, Moon], birthTime);
        }

        /// <summary>
        /// Definition: If there are planets (excepting the
        /// Sun) in the second house from the Moon, Sunapha
        /// is caused.
        /// 
        /// Results: Self-earned property, king, ruler or his
        /// equal, intelligent, wealthy and good reputation.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.SunaphaYoga)]
        public static CalculatorResult SunaphaYoga(Time birthTime)
        {
            //sun not in house 2 from moon
            var sunMoonDistance = Calculate.SignDistanceFromPlanetToPlanet(Moon, Sun, birthTime);
            var sunNotIn2 = sunMoonDistance != 2;

            //If there are planets
            //get sign 2nd house from moon
            var moon2ndHseSign = Calculate.SignCountedFromPlanetSign(2, Moon, birthTime);

            //get planets in that 2nd hse sign
            var planetsIn2 = Calculate.PlanetsInSign(moon2ndHseSign, birthTime);

            //both conditions have to be met
            var isOccuring = sunNotIn2 && planetsIn2.Any();

            return CalculatorResult.New(isOccuring, new[] { House2 }, new[] { Moon }, birthTime);
        }

        /// <summary>
        /// Definition: If there are planets in the 12th from
        /// the Moon, Anapha Yoga is formed.
        /// 
        /// Results: Well-formed organs, majestic appearance,
        /// good reputation, polite, generous, self-respect,
        /// fond of dress and sense pleasures. In later life,
        /// renunciation and austerity
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.AnaphaYoga)]
        public static CalculatorResult AnaphaYoga(Time birthTime)
        {
            //If there are planets in the 12th from moon
            var moon12ndHseSign = Calculate.SignCountedFromPlanetSign(12, Moon, birthTime);

            //get planets in that 12th hse sign from moon
            var planetsIn12 = Calculate.PlanetsInSign(moon12ndHseSign, birthTime);

            //Remarks.- In Anapha also the Sun is not taken
            //into account. The remarks made for Sunapha apply
            //to this also with slight variation.

            //remove sun if found
            planetsIn12.RemoveAll(x => x.Name == PlanetNameEnum.Sun);

            //both conditions have to be met
            var isOccuring = planetsIn12.Any();

            return CalculatorResult.New(isOccuring, new[] { House12 }, new[] { Moon }, birthTime);
        }

        /// <summary>
        /// Definition: If there are planets on either side of
        /// the Moon, the combination goes under the name of
        /// Dhurdhura.
        /// 
        /// Results: The native is bountiful. He will be
        /// blessed with much wealth and conveyances.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.DhurdhuraYoga)]
        public static CalculatorResult DhurdhuraYoga(Time birthTime)
        {
            //If there are planets on either side of the Moon
            var topSideSign = Calculate.SignCountedFromPlanetSign(2, Moon, birthTime);
            var planetsInTop = Calculate.PlanetsInSign(topSideSign, birthTime).Any();

            var bottomSideSign = Calculate.SignCountedFromPlanetSign(12, Moon, birthTime);
            var planetsInBottom = Calculate.PlanetsInSign(bottomSideSign, birthTime).Any();

            //on either side of  the Moon
            var planetOnBothSides = planetsInBottom || planetsInTop;

            return CalculatorResult.New(planetOnBothSides, new[] { Moon }, birthTime);
        }

        /// <summary>
        /// Definition: When there are no planets on both
        /// sides of the Moon, Kemadruma Yoga is formed
        /// 
        /// Results: The person will be dirty, sorrowful,
        /// doing unrighteous deeds, poor, dependent, a rogue
        /// and a swindler
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.KemadrumaYoga)]
        public static CalculatorResult KemadrumaYoga(Time birthTime)
        {
            //If there are planets on either side of the Moon
            //count to sign next to moon on right
            var topSideSign = Calculate.SignCountedFromPlanetSign(2, Moon, birthTime);
            var noPlanetsInTop = Calculate.PlanetsInSign(topSideSign, birthTime).Any() == false;

            //count around to sign left side of moon (since counter only goes one way)
            var bottomSideSign = Calculate.SignCountedFromPlanetSign(12, Moon, birthTime);
            var noPlanetsInBottom = Calculate.PlanetsInSign(bottomSideSign, birthTime).Any() == false;

            //no planets on both sides of the Moon
            var planetOnBothSides = noPlanetsInBottom && noPlanetsInTop;

            return CalculatorResult.New(planetOnBothSides, new[] { Moon }, birthTime);
        }

        /// <summary>
        /// Definition: If Mars conjoins the Moon this
        /// yoga is formed.
        /// 
        /// Results: Earnings through unscrupulous means,
        /// a seller of women, treating mother harshly and doing
        /// mischief to her and other relatives
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.ChandraMangalaYoga)]
        public static CalculatorResult ChandraMangalaYoga(Time birthTime)
        {
            //If Mars conjoins the Moon
            var marsConjunctMoon = Calculate.IsPlanetConjunctWithPlanet(Mars, Moon, birthTime);

            return CalculatorResult.New(marsConjunctMoon, new[] { Moon, Mars }, birthTime);
        }

        /// <summary>
        /// Definition: If benefics are situated in the 6th,
        /// 7th and 8th from the Moon, the combination goes
        /// under the name of Adhi Yoga
        /// 
        /// Results: The person will be polite and trustworthy, will have an enjoyable and happy life,
        /// surrounded by luxuries and affluence, will inflict
        /// defeats on his enemies, will be healthy and will live long.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.AdhiYoga)]
        public static CalculatorResult AdhiYoga(Time birthTime)
        {
            //If benefics are situated in the 6th,7th and 8th from the Moon
            int[] signsFromList = new[] { 6, 7, 8 };

            //Varahamihira distinctly observes Sounyehi-implying
            // clearly only the benefics, vz., Mercury, Jupiter and Venus.
            PlanetName[] beneficList = new[] { Mercury, Jupiter, Venus }; //override standard benefics

            var isOccuring = Calculate.IsPlanetsInSignsFromPlanet(signsFromList, beneficList, Moon, birthTime);

            return CalculatorResult.New(isOccuring, new[] { Moon }, birthTime);
        }

        /// <summary>
        /// Definition: Chatussagara Yoga is caused when
        /// all the kendras are occupied by planets
        /// 
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.ChatussagaraYoga)]
        public static CalculatorResult ChatussagaraYoga(Time birthTime)
        {
            var isOccuring = true; //start with is occuring

            //go through all the kendras if any one house,
            //does not have a planet, than exit and mark as not occuring
            //kendra house (1,4,7,10)
            var kendraList = new HouseName[] { House1, House4, House7, House10 };
            foreach (var house in kendraList)
            {
                //true if no planet found
                var noPlanet = Calculate.PlanetsInHouseBasedOnSign(house, birthTime).Any() == false;
                if (noPlanet) { isOccuring = false; break; } //set as not occuring, stop checking anymore

            }

            return CalculatorResult.New(isOccuring, kendraList, birthTime);
        }

        /// <summary>
        /// Definition: If benefics occupy the upachayas
        /// (3,6,10 and 11) either from the ascendant or from
        /// the Moon, the combination goes under the name of
        /// Vasumathi Yoga.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.VasumathiYoga)]
        public static CalculatorResult VasumathiYoga(Time birthTime)
        {
            //list upachayas houses (3,6,10 and 11)
            var upachayasList = new[] { 3, 6, 10, 11 };

            //check if there is benefics in upachayas from the ascendant/lagna
            var beneficsFoundInUpachFromLagna = Calculate.IsBeneficsInSignsFromLagna(upachayasList, birthTime);

            //check if there is benefics in upachayas from the Moon
            var beneficsFoundInUpachFromMoon = Calculate.IsBeneficsInSignsFromPlanet(upachayasList, Moon, birthTime);

            //for yoga to occur benefics has to be in either one above
            var isOccuring = beneficsFoundInUpachFromLagna || beneficsFoundInUpachFromMoon;

            //tell caller if Vasumathi Yoga is present in horoscope
            return CalculatorResult.New(isOccuring);
        }

        /// <summary>
        /// Definition: Jupiter, Venus, Mercury and the
        /// Moon should be in Lagna or they should be placed
        /// in kendra.
        /// 
        /// Results: The native will possess an attractive
        /// appearance and he will be endowed with all the
        /// good qualities of high personages.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.RajalakshanaYoga)]
        public static CalculatorResult RajalakshanaYoga(Time birthTime)
        {
            //planets to check
            var planetList = new[] { Jupiter, Venus, Mercury, Moon };

            //is any of the planet in Kendra
            var isInKendra = Calculate.IsPlanetInKendra(planetList, birthTime);

            //is any of the planet in Lagna/Ascendant
            var isInLagna = Calculate.IsAnyPlanetInHouse(planetList.ToList(), House1, birthTime);

            //is occurring if either is true
            var isOccuring = isInKendra || isInLagna;

            return CalculatorResult.New(isOccuring);
        }

        /// <summary>
        /// Definition: The Lagna is occupied by a malefic
        ///	with Gulika in a trine: or Gulika is associated with
        ///	the lords of Kendras and Thrikonas; or the lord of
        ///	Lagna is combined with Rahu, Saturn or Kethu.
        ///
        ///	Results: The native will always entertain feelings
        ///	of suspicion towards others around him. He is afraid
        ///	of being cheated, swindled and robbed.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.VanchanaChoraBheethiYoga)]
        public static CalculatorResult VanchanaChoraBheethiYoga(Time birthTime)
        {
            //todo not implemented yet!
            return CalculatorResult.NotOccuring();
        }

        /// <summary>
        /// Definition: The Moon in the 12th, 6th or 8th
        /// from Jupiter gives rise to Sakata Yoga
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.SakataYoga)]
        public static CalculatorResult SakataYoga(Time birthTime)
        {
            //sign counts to check (house)
            int[] signsFromList = new[] { 6, 8, 12 };

            //check for jupiter only
            PlanetName[] planetList = new[] { Moon };

            var isOccuring = Calculate.IsPlanetsInSignsFromPlanet(signsFromList, planetList, Jupiter, birthTime);

            return CalculatorResult.New(isOccuring, new[] { Moon, Jupiter }, birthTime);

        }

        /// <summary>
        /// Definition: The 10th from the Moon or Lagna should be occupied by a benefic planet
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.AmalaYoga)]
        public static CalculatorResult AmalaYoga(Time birthTime)
        {
            //The 10th from the Moon should be occupied by a benefic planet
            int[] _10thFrom = new[] { 10 };
            var _10thMoonIsBenefics = Calculate.IsBeneficsInSignsFromPlanet(_10thFrom, Moon, birthTime);

            //The 10th from the Lagna should be occupied by a benefic planet
            var _10thLagnaIsBenefics = Calculate.IsBeneficsInSignsFromLagna(_10thFrom, birthTime);

            //is occurring if either is true
            var isOccuring = _10thMoonIsBenefics || _10thLagnaIsBenefics;

            return CalculatorResult.New(isOccuring);

        }

        /// <summary>
        /// Definition: Benefics being disposed in Kendras,
        /// the 6th and 8th houses should either be unoccupied
        /// or occupied by benefic planets. This combination
        /// goes under the name of Parvata Yoga
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.ParvataYoga)]
        public static CalculatorResult ParvataYoga(Time birthTime)
        {
            //Benefics being disposed in Kendras
            var beneficsInKendra = Calculate.IsBeneficsInKendra(birthTime);

            //the 6th and 8th must be free
            var _6thEmpty = Calculate.PlanetsInHouseBasedOnSign(House6, birthTime).Count == 0;
            var _8thEmpty = Calculate.PlanetsInHouseBasedOnSign(House8, birthTime).Count == 0;
            var _6th8thIsEmpty = _6thEmpty && _8thEmpty;

            //the 6th and 8th occupied by benefics
            var _6thHasBenefics = Calculate.IsBeneficPlanetInHouse(House6, birthTime);
            var _8thHasBenefics = Calculate.IsBeneficPlanetInHouse(House8, birthTime);
            var _6th8thHasBenefics = _6thHasBenefics && _8thHasBenefics;

            //is occurring if conditions meet
            var isOccuring = beneficsInKendra && (_6th8thIsEmpty || _6th8thHasBenefics);

            return CalculatorResult.New(isOccuring);

        }

        /// <summary>
        /// Definition:  Lords of the fourth and ninth houses
        /// should be in Kendras from each other and the lord
        /// of Lagna should be strongly disposed.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.KahalaYoga)]
        public static CalculatorResult KahalaYoga(Time birthTime)
        {
            //# Condition 1
            //get lords of the 4th and 9th houses
            var lordOf4th = Calculate.LordOfHouse(House4, birthTime);
            var lordOf9th = Calculate.LordOfHouse(House9, birthTime);

            //should be in Kendras from each other
            var possibleKendra1 = Calculate.IsPlanetInKendraFromPlanet(lordOf9th, lordOf4th, birthTime);
            var possibleKendra2 = Calculate.IsPlanetInKendraFromPlanet(lordOf4th, lordOf9th, birthTime);
            var isKendraFromEachOther = possibleKendra1 || possibleKendra2;

            //# Condition 2
            //lord of Lagna should be strongly disposed.
            var lagnaLord = Calculate.LordOfHouse(House1, birthTime);

            //strength here is based on minimum point set by BV Raman in Bala book
            var lagnaLordIsStrong = Calculate.IsPlanetStrongInShadbala(lagnaLord, birthTime);

            //is occurring if conditions meet
            var isOccuring = isKendraFromEachOther && lagnaLordIsStrong;

            return CalculatorResult.New(isOccuring);
        }

        /// <summary>
        /// If planets other than the Moon
        /// occupy the 2nd from the Sun, Vesi Yoga is formed.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.VesiYoga)]
        public static CalculatorResult VesiYoga(Time birthTime)
        {
            // If planets other than the Moon
            // occupy the 2nd from the Sun, Vesi Yoga is formed.

            // Get the 2nd house from the Sun
            var secondHouseFromSun = Calculate.SignCountedFromPlanetSign(2, PlanetName.Sun, birthTime);

            // Check if planets other than the Moon occupy the 2nd house from the Sun
            var planetsIn2ndHouse = Calculate.PlanetsInSign(secondHouseFromSun, birthTime);
            //var moonSign = Calculate.SignCountedFromPlanetSign(0, PlanetName.Moon, birthTime);

            // Check if any planet except Moon is in 2nd house
            var isOccuring = planetsIn2ndHouse.Any(p => p != PlanetName.Moon);

            return CalculatorResult.New(isOccuring);
        }

        /// <summary>
        /// Planets other than the Moon occupying the 12th from the Sun gives rise to Vasi Yoga.
        /// </summary>
        [HoroscopeCalculator(HoroscopeName.VasiYoga)]
        public static CalculatorResult VasiYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.ObhayachariYoga)]
        public static CalculatorResult ObhayachariYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.HamsaYoga)]
        public static CalculatorResult HamsaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.MalavyaYoga)]
        public static CalculatorResult MalavyaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.SasaYoga)]
        public static CalculatorResult SasaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.RuchakaYoga)]
        public static CalculatorResult RuchakaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.BhadraYoga)]
        public static CalculatorResult BhadraYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.BudhaAdityaYoga)]
        public static CalculatorResult BudhaAdityaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.MahabhagyaYogaMale)]
        public static CalculatorResult MahabhagyaYogaMale(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.MahabhagyaYogaFemale)]
        public static CalculatorResult MahabhagyaYogaFemale(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.PushkalaYoga)]
        public static CalculatorResult PushkalaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.LakshmiYoga)]
        public static CalculatorResult LakshmiYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.GauriYoga)]
        public static CalculatorResult GauriYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.BharathiYoga)]
        public static CalculatorResult BharathiYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.ChapaYoga)]
        public static CalculatorResult ChapaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.SreenathaYoga)]
        public static CalculatorResult SreenathaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.LagnaMalikaYoga)]
        public static CalculatorResult LagnaMalikaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.DhanaMalikaYoga)]
        public static CalculatorResult DhanaMalikaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.VikramaMalikaYoga)]
        public static CalculatorResult VikramaMalikaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.SukhaMalikaYoga)]
        public static CalculatorResult SukhaMalikaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.PutraMalikaYoga)]
        public static CalculatorResult PutraMalikaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.SatruMalikaYoga)]
        public static CalculatorResult SatruMalikaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.KalatraMalikaYoga)]
        public static CalculatorResult KalatraMalikaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.RandhraMalikaYoga)]
        public static CalculatorResult RandhraMalikaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.BhagyaMalikaYoga)]
        public static CalculatorResult BhagyaMalikaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.KarmaMalikaYoga)]
        public static CalculatorResult KarmaMalikaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }

        [HoroscopeCalculator(HoroscopeName.MalikaYoga)]
        public static CalculatorResult MalikaYoga(Time birthTime)
        {
            return CalculatorResult.New(false);
        }




        #endregion

        #region PLANETS IN HOUSES

        #region Planets in the 1st House

        [HoroscopeCalculator(HoroscopeName.SunInHouse1)]
        public static CalculatorResult SunInHouse1(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Sun, HouseName.House1, time), new[] { HouseName.House1, }, new[] { PlanetName.Sun }, time);

        [HoroscopeCalculator(HoroscopeName.MoonInHouse1)]
        public static CalculatorResult MoonInHouse1(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Moon, HouseName.House1, time), new[] { HouseName.House1, }, new[] { PlanetName.Moon }, time);

        [HoroscopeCalculator(HoroscopeName.MarsInHouse1)]
        public static CalculatorResult MarsInHouse1(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mars, HouseName.House1, time), new[] { HouseName.House1, }, new[] { PlanetName.Mars }, time);

        [HoroscopeCalculator(HoroscopeName.MercuryInHouse1)]
        public static CalculatorResult MercuryInHouse1(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mercury, HouseName.House1, time), new[] { HouseName.House1, }, new[] { PlanetName.Mercury }, time);

        [HoroscopeCalculator(HoroscopeName.JupiterInHouse1)]
        public static CalculatorResult JupiterInHouse1(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Jupiter, HouseName.House1, time), new[] { HouseName.House1, }, new[] { PlanetName.Jupiter }, time);

        [HoroscopeCalculator(HoroscopeName.VenusInHouse1)]
        public static CalculatorResult VenusInHouse1(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Venus, HouseName.House1, time), new[] { HouseName.House1, }, new[] { PlanetName.Venus }, time);

        [HoroscopeCalculator(HoroscopeName.SaturnInHouse1)]
        public static CalculatorResult SaturnInHouse1(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Saturn, HouseName.House1, time), new[] { HouseName.House1, }, new[] { PlanetName.Saturn }, time);

        [HoroscopeCalculator(HoroscopeName.RahuInHouse1)]
        public static CalculatorResult RahuInHouse1(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Rahu, HouseName.House1, time), new[] { HouseName.House1, }, new[] { PlanetName.Rahu }, time);

        [HoroscopeCalculator(HoroscopeName.KetuInHouse1)]
        public static CalculatorResult KetuInHouse1(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Ketu, HouseName.House1, time), new[] { HouseName.House1, }, new[] { PlanetName.Ketu }, time);

        #endregion

        #region Planets in the 2nd House

        [HoroscopeCalculator(HoroscopeName.SunInHouse2)]
        public static CalculatorResult SunInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Sun, HouseName.House2, time), new[] { HouseName.House2, }, new[] { PlanetName.Sun }, time);

        [HoroscopeCalculator(HoroscopeName.MoonInHouse2)]
        public static CalculatorResult MoonInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Moon, HouseName.House2, time), new[] { HouseName.House2, }, new[] { PlanetName.Moon }, time);

        [HoroscopeCalculator(HoroscopeName.MarsInHouse2)]
        public static CalculatorResult MarsInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mars, HouseName.House2, time), new[] { HouseName.House2, }, new[] { PlanetName.Mars }, time);

        [HoroscopeCalculator(HoroscopeName.MercuryInHouse2)]
        public static CalculatorResult MercuryInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mercury, HouseName.House2, time), new[] { HouseName.House2, }, new[] { PlanetName.Mercury }, time);

        [HoroscopeCalculator(HoroscopeName.JupiterInHouse2)]
        public static CalculatorResult JupiterInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Jupiter, HouseName.House2, time), new[] { HouseName.House2, }, new[] { PlanetName.Jupiter }, time);

        [HoroscopeCalculator(HoroscopeName.VenusInHouse2)]
        public static CalculatorResult VenusInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Venus, HouseName.House2, time), new[] { HouseName.House2, }, new[] { PlanetName.Venus }, time);

        [HoroscopeCalculator(HoroscopeName.SaturnInHouse2)]
        public static CalculatorResult SaturnInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Saturn, HouseName.House2, time), new[] { HouseName.House2, }, new[] { PlanetName.Saturn }, time);

        [HoroscopeCalculator(HoroscopeName.RahuInHouse2)]
        public static CalculatorResult RahuInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Rahu, HouseName.House2, time), new[] { HouseName.House2, }, new[] { PlanetName.Rahu }, time);

        [HoroscopeCalculator(HoroscopeName.KetuInHouse2)]
        public static CalculatorResult KetuInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Ketu, HouseName.House2, time), new[] { HouseName.House2, }, new[] { PlanetName.Ketu }, time);

        #endregion

        #region Planets in the 3rd House

        [HoroscopeCalculator(HoroscopeName.SunInHouse3)]
        public static CalculatorResult SunInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Sun, HouseName.House3, time), new[] { HouseName.House3, }, new[] { PlanetName.Sun }, time);

        [HoroscopeCalculator(HoroscopeName.MoonInHouse3)]
        public static CalculatorResult MoonInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Moon, HouseName.House3, time), new[] { HouseName.House3, }, new[] { PlanetName.Moon }, time);

        [HoroscopeCalculator(HoroscopeName.MarsInHouse3)]
        public static CalculatorResult MarsInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mars, HouseName.House3, time), new[] { HouseName.House3, }, new[] { PlanetName.Mars }, time);

        [HoroscopeCalculator(HoroscopeName.MercuryInHouse3)]
        public static CalculatorResult MercuryInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mercury, HouseName.House3, time), new[] { HouseName.House3, }, new[] { PlanetName.Mercury }, time);

        [HoroscopeCalculator(HoroscopeName.JupiterInHouse3)]
        public static CalculatorResult JupiterInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Jupiter, HouseName.House3, time), new[] { HouseName.House3, }, new[] { PlanetName.Jupiter }, time);

        [HoroscopeCalculator(HoroscopeName.VenusInHouse3)]
        public static CalculatorResult VenusInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Venus, HouseName.House3, time), new[] { HouseName.House3, }, new[] { PlanetName.Venus }, time);

        [HoroscopeCalculator(HoroscopeName.SaturnInHouse3)]
        public static CalculatorResult SaturnInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Saturn, HouseName.House3, time), new[] { HouseName.House3, }, new[] { PlanetName.Saturn }, time);

        [HoroscopeCalculator(HoroscopeName.RahuInHouse3)]
        public static CalculatorResult RahuInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Rahu, HouseName.House3, time), new[] { HouseName.House3, }, new[] { PlanetName.Rahu }, time);

        [HoroscopeCalculator(HoroscopeName.KetuInHouse3)]
        public static CalculatorResult KetuInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Ketu, HouseName.House3, time), new[] { HouseName.House3, }, new[] { PlanetName.Ketu }, time);

        #endregion

        #region Planets in the 4th House

        [HoroscopeCalculator(HoroscopeName.SunInHouse4)]
        public static CalculatorResult SunInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Sun, HouseName.House4, time), new[] { HouseName.House4, }, new[] { PlanetName.Sun }, time);

        [HoroscopeCalculator(HoroscopeName.MoonInHouse4)]
        public static CalculatorResult MoonInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Moon, HouseName.House4, time), new[] { HouseName.House4, }, new[] { PlanetName.Moon }, time);

        [HoroscopeCalculator(HoroscopeName.MarsInHouse4)]
        public static CalculatorResult MarsInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mars, HouseName.House4, time), new[] { HouseName.House4, }, new[] { PlanetName.Mars }, time);

        [HoroscopeCalculator(HoroscopeName.MercuryInHouse4)]
        public static CalculatorResult MercuryInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mercury, HouseName.House4, time), new[] { HouseName.House4, }, new[] { PlanetName.Mercury }, time);

        [HoroscopeCalculator(HoroscopeName.JupiterInHouse4)]
        public static CalculatorResult JupiterInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Jupiter, HouseName.House4, time), new[] { HouseName.House4, }, new[] { PlanetName.Jupiter }, time);

        [HoroscopeCalculator(HoroscopeName.VenusInHouse4)]
        public static CalculatorResult VenusInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Venus, HouseName.House4, time), new[] { HouseName.House4, }, new[] { PlanetName.Venus }, time);

        [HoroscopeCalculator(HoroscopeName.SaturnInHouse4)]
        public static CalculatorResult SaturnInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Saturn, HouseName.House4, time), new[] { HouseName.House4, }, new[] { PlanetName.Saturn }, time);

        [HoroscopeCalculator(HoroscopeName.RahuInHouse4)]
        public static CalculatorResult RahuInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Rahu, HouseName.House4, time), new[] { HouseName.House4, }, new[] { PlanetName.Rahu }, time);

        [HoroscopeCalculator(HoroscopeName.KetuInHouse4)]
        public static CalculatorResult KetuInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Ketu, HouseName.House4, time), new[] { HouseName.House4, }, new[] { PlanetName.Ketu }, time);

        #endregion

        #region Planets in the 5th House

        [HoroscopeCalculator(HoroscopeName.SunInHouse5)]
        public static CalculatorResult SunInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Sun, HouseName.House5, time), new[] { HouseName.House5, }, new[] { PlanetName.Sun }, time);

        [HoroscopeCalculator(HoroscopeName.MoonInHouse5)]
        public static CalculatorResult MoonInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Moon, HouseName.House5, time), new[] { HouseName.House5, }, new[] { PlanetName.Moon }, time);

        [HoroscopeCalculator(HoroscopeName.MarsInHouse5)]
        public static CalculatorResult MarsInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mars, HouseName.House5, time), new[] { HouseName.House5, }, new[] { PlanetName.Mars }, time);

        [HoroscopeCalculator(HoroscopeName.MercuryInHouse5)]
        public static CalculatorResult MercuryInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mercury, HouseName.House5, time), new[] { HouseName.House5, }, new[] { PlanetName.Mercury }, time);

        [HoroscopeCalculator(HoroscopeName.JupiterInHouse5)]
        public static CalculatorResult JupiterInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Jupiter, HouseName.House5, time), new[] { HouseName.House5, }, new[] { PlanetName.Jupiter }, time);

        [HoroscopeCalculator(HoroscopeName.VenusInHouse5)]
        public static CalculatorResult VenusInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Venus, HouseName.House5, time), new[] { HouseName.House5, }, new[] { PlanetName.Venus }, time);

        [HoroscopeCalculator(HoroscopeName.SaturnInHouse5)]
        public static CalculatorResult SaturnInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Saturn, HouseName.House5, time), new[] { HouseName.House5, }, new[] { PlanetName.Saturn }, time);

        [HoroscopeCalculator(HoroscopeName.RahuInHouse5)]
        public static CalculatorResult RahuInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Rahu, HouseName.House5, time), new[] { HouseName.House5, }, new[] { PlanetName.Rahu }, time);

        [HoroscopeCalculator(HoroscopeName.KetuInHouse5)]
        public static CalculatorResult KetuInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Ketu, HouseName.House5, time), new[] { HouseName.House5, }, new[] { PlanetName.Ketu }, time);

        #endregion

        #region Planets in the 6th House

        [HoroscopeCalculator(HoroscopeName.SunInHouse6)]
        public static CalculatorResult SunInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Sun, HouseName.House6, time), new[] { HouseName.House6, }, new[] { PlanetName.Sun }, time);

        [HoroscopeCalculator(HoroscopeName.MoonInHouse6)]
        public static CalculatorResult MoonInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Moon, HouseName.House6, time), new[] { HouseName.House6, }, new[] { PlanetName.Moon }, time);

        [HoroscopeCalculator(HoroscopeName.MarsInHouse6)]
        public static CalculatorResult MarsInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mars, HouseName.House6, time), new[] { HouseName.House6, }, new[] { PlanetName.Mars }, time);

        [HoroscopeCalculator(HoroscopeName.MercuryInHouse6)]
        public static CalculatorResult MercuryInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mercury, HouseName.House6, time), new[] { HouseName.House6, }, new[] { PlanetName.Mercury }, time);

        [HoroscopeCalculator(HoroscopeName.JupiterInHouse6)]
        public static CalculatorResult JupiterInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Jupiter, HouseName.House6, time), new[] { HouseName.House6, }, new[] { PlanetName.Jupiter }, time);

        [HoroscopeCalculator(HoroscopeName.VenusInHouse6)]
        public static CalculatorResult VenusInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Venus, HouseName.House6, time), new[] { HouseName.House6, }, new[] { PlanetName.Venus }, time);

        [HoroscopeCalculator(HoroscopeName.SaturnInHouse6)]
        public static CalculatorResult SaturnInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Saturn, HouseName.House6, time), new[] { HouseName.House6, }, new[] { PlanetName.Saturn }, time);

        [HoroscopeCalculator(HoroscopeName.RahuInHouse6)]
        public static CalculatorResult RahuInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Rahu, HouseName.House6, time), new[] { HouseName.House6, }, new[] { PlanetName.Rahu }, time);

        [HoroscopeCalculator(HoroscopeName.KetuInHouse6)]
        public static CalculatorResult KetuInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Ketu, HouseName.House6, time), new[] { HouseName.House6, }, new[] { PlanetName.Ketu }, time);


        //Planets in the 7th House

        [HoroscopeCalculator(HoroscopeName.SunInHouse7)]
        public static CalculatorResult SunInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Sun, HouseName.House7, time), new[] { HouseName.House7, }, new[] { PlanetName.Sun }, time);

        [HoroscopeCalculator(HoroscopeName.MoonInHouse7)]
        public static CalculatorResult MoonInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Moon, HouseName.House7, time), new[] { HouseName.House7, }, new[] { PlanetName.Moon }, time);

        [HoroscopeCalculator(HoroscopeName.MarsInHouse7)]
        public static CalculatorResult MarsInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mars, HouseName.House7, time), new[] { HouseName.House7, }, new[] { PlanetName.Mars }, time);

        [HoroscopeCalculator(HoroscopeName.MercuryInHouse7)]
        public static CalculatorResult MercuryInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mercury, HouseName.House7, time), new[] { HouseName.House7, }, new[] { PlanetName.Mercury }, time);

        [HoroscopeCalculator(HoroscopeName.JupiterInHouse7)]
        public static CalculatorResult JupiterInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Jupiter, HouseName.House7, time), new[] { HouseName.House7, }, new[] { PlanetName.Jupiter }, time);

        [HoroscopeCalculator(HoroscopeName.VenusInHouse7)]
        public static CalculatorResult VenusInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Venus, HouseName.House7, time), new[] { HouseName.House7, }, new[] { PlanetName.Venus }, time);

        [HoroscopeCalculator(HoroscopeName.SaturnInHouse7)]
        public static CalculatorResult SaturnInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Saturn, HouseName.House7, time), new[] { HouseName.House7, }, new[] { PlanetName.Saturn }, time);

        [HoroscopeCalculator(HoroscopeName.RahuInHouse7)]
        public static CalculatorResult RahuInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Rahu, HouseName.House7, time), new[] { HouseName.House7, }, new[] { PlanetName.Rahu }, time);

        [HoroscopeCalculator(HoroscopeName.KetuInHouse7)]
        public static CalculatorResult KetuInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Ketu, HouseName.House7, time), new[] { HouseName.House7, }, new[] { PlanetName.Ketu }, time);

        #endregion

        #region Planets in the 8th House

        [HoroscopeCalculator(HoroscopeName.SunInHouse8)]
        public static CalculatorResult SunInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Sun, HouseName.House8, time), new[] { HouseName.House8, }, new[] { PlanetName.Sun }, time);

        [HoroscopeCalculator(HoroscopeName.MoonInHouse8)]
        public static CalculatorResult MoonInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Moon, HouseName.House8, time), new[] { HouseName.House8, }, new[] { PlanetName.Moon }, time);

        [HoroscopeCalculator(HoroscopeName.MarsInHouse8)]
        public static CalculatorResult MarsInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mars, HouseName.House8, time), new[] { HouseName.House8, }, new[] { PlanetName.Mars }, time);

        [HoroscopeCalculator(HoroscopeName.MercuryInHouse8)]
        public static CalculatorResult MercuryInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mercury, HouseName.House8, time), new[] { HouseName.House8, }, new[] { PlanetName.Mercury }, time);

        [HoroscopeCalculator(HoroscopeName.JupiterInHouse8)]
        public static CalculatorResult JupiterInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Jupiter, HouseName.House8, time), new[] { HouseName.House8, }, new[] { PlanetName.Jupiter }, time);

        [HoroscopeCalculator(HoroscopeName.VenusInHouse8)]
        public static CalculatorResult VenusInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Venus, HouseName.House8, time), new[] { HouseName.House8, }, new[] { PlanetName.Venus }, time);

        [HoroscopeCalculator(HoroscopeName.SaturnInHouse8)]
        public static CalculatorResult SaturnInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Saturn, HouseName.House8, time), new[] { HouseName.House8, }, new[] { PlanetName.Saturn }, time);

        [HoroscopeCalculator(HoroscopeName.RahuInHouse8)]
        public static CalculatorResult RahuInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Rahu, HouseName.House8, time), new[] { HouseName.House8, }, new[] { PlanetName.Rahu }, time);

        [HoroscopeCalculator(HoroscopeName.KetuInHouse8)]
        public static CalculatorResult KetuInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Ketu, HouseName.House8, time), new[] { HouseName.House8, }, new[] { PlanetName.Ketu }, time);


        #endregion

        #region Planets in the 9th House

        [HoroscopeCalculator(HoroscopeName.SunInHouse9)]
        public static CalculatorResult SunInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Sun, HouseName.House9, time), new[] { HouseName.House9, }, new[] { PlanetName.Sun }, time);

        [HoroscopeCalculator(HoroscopeName.MoonInHouse9)]
        public static CalculatorResult MoonInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Moon, HouseName.House9, time), new[] { HouseName.House9, }, new[] { PlanetName.Moon }, time);

        [HoroscopeCalculator(HoroscopeName.MarsInHouse9)]
        public static CalculatorResult MarsInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mars, HouseName.House9, time), new[] { HouseName.House9, }, new[] { PlanetName.Mars }, time);

        [HoroscopeCalculator(HoroscopeName.MercuryInHouse9)]
        public static CalculatorResult MercuryInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mercury, HouseName.House9, time), new[] { HouseName.House9, }, new[] { PlanetName.Mercury }, time);

        [HoroscopeCalculator(HoroscopeName.JupiterInHouse9)]
        public static CalculatorResult JupiterInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Jupiter, HouseName.House9, time), new[] { HouseName.House9, }, new[] { PlanetName.Jupiter }, time);

        [HoroscopeCalculator(HoroscopeName.VenusInHouse9)]
        public static CalculatorResult VenusInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Venus, HouseName.House9, time), new[] { HouseName.House9, }, new[] { PlanetName.Venus }, time);

        [HoroscopeCalculator(HoroscopeName.SaturnInHouse9)]
        public static CalculatorResult SaturnInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Saturn, HouseName.House9, time), new[] { HouseName.House9, }, new[] { PlanetName.Saturn }, time);

        [HoroscopeCalculator(HoroscopeName.RahuInHouse9)]
        public static CalculatorResult RahuInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Rahu, HouseName.House9, time), new[] { HouseName.House9, }, new[] { PlanetName.Rahu }, time);

        [HoroscopeCalculator(HoroscopeName.KetuInHouse9)]
        public static CalculatorResult KetuInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Ketu, HouseName.House9, time), new[] { HouseName.House9, }, new[] { PlanetName.Ketu }, time);

        #endregion

        #region Planets in the 10th House

        [HoroscopeCalculator(HoroscopeName.SunInHouse10)]
        public static CalculatorResult SunInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Sun, HouseName.House10, time), new[] { HouseName.House10, }, new[] { PlanetName.Sun }, time);

        [HoroscopeCalculator(HoroscopeName.MoonInHouse10)]
        public static CalculatorResult MoonInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Moon, HouseName.House10, time), new[] { HouseName.House10, }, new[] { PlanetName.Moon }, time);

        [HoroscopeCalculator(HoroscopeName.MarsInHouse10)]
        public static CalculatorResult MarsInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mars, HouseName.House10, time), new[] { HouseName.House10, }, new[] { PlanetName.Mars }, time);

        [HoroscopeCalculator(HoroscopeName.MercuryInHouse10)]
        public static CalculatorResult MercuryInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mercury, HouseName.House10, time), new[] { HouseName.House10, }, new[] { PlanetName.Mercury }, time);

        [HoroscopeCalculator(HoroscopeName.JupiterInHouse10)]
        public static CalculatorResult JupiterInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Jupiter, HouseName.House10, time), new[] { HouseName.House10, }, new[] { PlanetName.Jupiter }, time);

        [HoroscopeCalculator(HoroscopeName.VenusInHouse10)]
        public static CalculatorResult VenusInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Venus, HouseName.House10, time), new[] { HouseName.House10, }, new[] { PlanetName.Venus }, time);

        [HoroscopeCalculator(HoroscopeName.SaturnInHouse10)]
        public static CalculatorResult SaturnInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Saturn, HouseName.House10, time), new[] { HouseName.House10, }, new[] { PlanetName.Saturn }, time);

        [HoroscopeCalculator(HoroscopeName.RahuInHouse10)]
        public static CalculatorResult RahuInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Rahu, HouseName.House10, time), new[] { HouseName.House10, }, new[] { PlanetName.Rahu }, time);

        [HoroscopeCalculator(HoroscopeName.KetuInHouse10)]
        public static CalculatorResult KetuInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Ketu, HouseName.House10, time), new[] { HouseName.House10, }, new[] { PlanetName.Ketu }, time);

        #endregion

        #region Planets in the 11th House

        [HoroscopeCalculator(HoroscopeName.SunInHouse11)]
        public static CalculatorResult SunInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Sun, HouseName.House11, time), new[] { HouseName.House11, }, new[] { PlanetName.Sun }, time);

        [HoroscopeCalculator(HoroscopeName.MoonInHouse11)]
        public static CalculatorResult MoonInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Moon, HouseName.House11, time), new[] { HouseName.House11, }, new[] { PlanetName.Moon }, time);

        [HoroscopeCalculator(HoroscopeName.MarsInHouse11)]
        public static CalculatorResult MarsInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mars, HouseName.House11, time), new[] { HouseName.House11, }, new[] { PlanetName.Mars }, time);

        [HoroscopeCalculator(HoroscopeName.MercuryInHouse11)]
        public static CalculatorResult MercuryInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mercury, HouseName.House11, time), new[] { HouseName.House11, }, new[] { PlanetName.Mercury }, time);

        [HoroscopeCalculator(HoroscopeName.JupiterInHouse11)]
        public static CalculatorResult JupiterInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Jupiter, HouseName.House11, time), new[] { HouseName.House11, }, new[] { PlanetName.Jupiter }, time);

        [HoroscopeCalculator(HoroscopeName.VenusInHouse11)]
        public static CalculatorResult VenusInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Venus, HouseName.House11, time), new[] { HouseName.House11, }, new[] { PlanetName.Venus }, time);

        [HoroscopeCalculator(HoroscopeName.SaturnInHouse11)]
        public static CalculatorResult SaturnInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Saturn, HouseName.House11, time), new[] { HouseName.House11, }, new[] { PlanetName.Saturn }, time);

        [HoroscopeCalculator(HoroscopeName.RahuInHouse11)]
        public static CalculatorResult RahuInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Rahu, HouseName.House11, time), new[] { HouseName.House11, }, new[] { PlanetName.Rahu }, time);

        [HoroscopeCalculator(HoroscopeName.KetuInHouse11)]
        public static CalculatorResult KetuInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Ketu, HouseName.House11, time), new[] { HouseName.House11, }, new[] { PlanetName.Ketu }, time);

        #endregion

        #region Planets in the 12th House

        [HoroscopeCalculator(HoroscopeName.SunInHouse12)]
        public static CalculatorResult SunInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Sun, HouseName.House12, time), new[] { HouseName.House12 }, new[] { PlanetName.Sun }, time);

        [HoroscopeCalculator(HoroscopeName.MoonInHouse12)]
        public static CalculatorResult MoonInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Moon, HouseName.House12, time), new[] { HouseName.House12 }, new[] { PlanetName.Moon }, time);

        [HoroscopeCalculator(HoroscopeName.MarsInHouse12)]
        public static CalculatorResult MarsInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mars, HouseName.House12, time), new[] { HouseName.House12 }, new[] { PlanetName.Mars }, time);

        [HoroscopeCalculator(HoroscopeName.MercuryInHouse12)]
        public static CalculatorResult MercuryInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Mercury, HouseName.House12, time), new[] { HouseName.House12 }, new[] { PlanetName.Mercury }, time);

        [HoroscopeCalculator(HoroscopeName.JupiterInHouse12)]
        public static CalculatorResult JupiterInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Jupiter, HouseName.House12, time), new[] { HouseName.House12 }, new[] { PlanetName.Jupiter }, time);

        [HoroscopeCalculator(HoroscopeName.VenusInHouse12)]
        public static CalculatorResult VenusInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Venus, HouseName.House12, time), new[] { HouseName.House12 }, new[] { PlanetName.Venus }, time);

        [HoroscopeCalculator(HoroscopeName.SaturnInHouse12)]
        public static CalculatorResult SaturnInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Saturn, HouseName.House12, time), new[] { HouseName.House12 }, new[] { PlanetName.Saturn }, time);

        [HoroscopeCalculator(HoroscopeName.RahuInHouse12)]
        public static CalculatorResult RahuInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Rahu, HouseName.House12, time), new[] { HouseName.House12 }, new[] { PlanetName.Rahu }, time);

        [HoroscopeCalculator(HoroscopeName.KetuInHouse12)]
        public static CalculatorResult KetuInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsPlanetInHouse(PlanetName.Ketu, HouseName.House12, time), new[] { HouseName.House12 }, new[] { PlanetName.Ketu }, time);

        #endregion

        #endregion

        #region LORD IN DIFFERENT HOUSES

        #region Lord of 1st being Situated in Different Houses

        [HoroscopeCalculator(HoroscopeName.House1LordInHouse1Fortified)]
        public static CalculatorResult House1LordInHouse1Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House1, HouseName.House1, time), new[] { HouseName.House1, HouseName.House1 }, time);

        [HoroscopeCalculator(HoroscopeName.House1LordInHouse2)]
        public static CalculatorResult House1LordInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House1, HouseName.House2, time), new[] { HouseName.House1, HouseName.House2 }, time);

        [HoroscopeCalculator(HoroscopeName.House1LordInHouse3)]
        public static CalculatorResult House1LordInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House1, HouseName.House3, time), new[] { HouseName.House1, HouseName.House3 }, time);

        [HoroscopeCalculator(HoroscopeName.House1LordInHouse4)]
        public static CalculatorResult House1LordInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House1, HouseName.House4, time), new[] { HouseName.House1, HouseName.House4 }, time);

        [HoroscopeCalculator(HoroscopeName.House1LordInHouse5)]
        public static CalculatorResult House1LordInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House1, HouseName.House5, time), new[] { HouseName.House1, HouseName.House5 }, time);

        [HoroscopeCalculator(HoroscopeName.House1LordInHouse6)]
        public static CalculatorResult House1LordInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House1, HouseName.House6, time), new[] { HouseName.House1, HouseName.House6 }, time);

        [HoroscopeCalculator(HoroscopeName.House1LordInHouse7)]
        public static CalculatorResult House1LordInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House1, HouseName.House7, time), new[] { HouseName.House1, HouseName.House7 }, time);

        [HoroscopeCalculator(HoroscopeName.House1LordInHouse8)]
        public static CalculatorResult House1LordInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House1, HouseName.House8, time), new[] { HouseName.House1, HouseName.House8 }, time);

        [HoroscopeCalculator(HoroscopeName.House1LordInHouse9)]
        public static CalculatorResult House1LordInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House1, HouseName.House9, time), new[] { HouseName.House1, HouseName.House9 }, time);

        [HoroscopeCalculator(HoroscopeName.House1LordInHouse10)]
        public static CalculatorResult House1LordInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House1, HouseName.House10, time), new[] { HouseName.House1, HouseName.House10 }, time);

        [HoroscopeCalculator(HoroscopeName.House1LordInHouse11)]
        public static CalculatorResult House1LordInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House1, HouseName.House11, time), new[] { HouseName.House1, HouseName.House11 }, time);

        [HoroscopeCalculator(HoroscopeName.House1LordInHouse12)]
        public static CalculatorResult House1LordInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House1, HouseName.House12, time), new[] { HouseName.House1, HouseName.House12 }, time);

        #endregion

        #region Lord of 2nd being Situated in Different Houses

        [HoroscopeCalculator(HoroscopeName.House2LordInHouse1)]
        public static CalculatorResult House2LordInHouse1Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House2, HouseName.House1, time), new[] { HouseName.House2, HouseName.House1 }, time);

        [HoroscopeCalculator(HoroscopeName.House2LordInHouse1With9thOrSun)]
        public static CalculatorResult House2LordInHouse1With9thOrSun(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House2, HouseName.House1, time), new[] { HouseName.House2, HouseName.House1 }, time);

        [HoroscopeCalculator(HoroscopeName.House2LordInHouse2)]
        public static CalculatorResult House2LordInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House2, HouseName.House2, time), new[] { HouseName.House2, HouseName.House2 }, time);

        [HoroscopeCalculator(HoroscopeName.House2LordInHouse3)]
        public static CalculatorResult House2LordInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House2, HouseName.House3, time), new[] { HouseName.House2, HouseName.House3 }, time);

        [HoroscopeCalculator(HoroscopeName.House2LordInHouse4)]
        public static CalculatorResult House2LordInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House2, HouseName.House4, time), new[] { HouseName.House2, HouseName.House4 }, time);

        [HoroscopeCalculator(HoroscopeName.House2LordInHouse4Afflicted)]
        public static CalculatorResult House2LordInHouse4Afflicted(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House2, HouseName.House4, time), new[] { HouseName.House2, HouseName.House4 }, time);

        [HoroscopeCalculator(HoroscopeName.House2LordInHouse4Fortified)]
        public static CalculatorResult House2LordInHouse4Fortified(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House2, HouseName.House4, time), new[] { HouseName.House2, HouseName.House4 }, time);

        [HoroscopeCalculator(HoroscopeName.House2LordInHouse5)]
        public static CalculatorResult House2LordInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House2, HouseName.House5, time), new[] { HouseName.House2, HouseName.House5 }, time);

        [HoroscopeCalculator(HoroscopeName.House2LordInHouse6Fortified)]
        public static CalculatorResult House2LordInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House2, HouseName.House6, time), new[] { HouseName.House2, HouseName.House6 }, time);

        [HoroscopeCalculator(HoroscopeName.House2LordInHouse6Afflicted)]
        public static CalculatorResult House2LordInHouse6Afflicted(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House2, HouseName.House6, time), new[] { HouseName.House2, HouseName.House6 }, time);

        [HoroscopeCalculator(HoroscopeName.House2LordInHouse7)]
        public static CalculatorResult House2LordInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House2, HouseName.House7, time), new[] { HouseName.House2, HouseName.House7 }, time);

        [HoroscopeCalculator(HoroscopeName.House2LordInHouse8)]
        public static CalculatorResult House2LordInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House2, HouseName.House8, time), new[] { HouseName.House2, HouseName.House8 }, time);

        [HoroscopeCalculator(HoroscopeName.House2LordInHouse9)]
        public static CalculatorResult House2LordInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House2, HouseName.House9, time), new[] { HouseName.House2, HouseName.House9 }, time);

        [HoroscopeCalculator(HoroscopeName.House2LordInHouse10)]
        public static CalculatorResult House2LordInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House2, HouseName.House10, time), new[] { HouseName.House2, HouseName.House10 }, time);

        [HoroscopeCalculator(HoroscopeName.House2LordInHouse11)]
        public static CalculatorResult House2LordInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House2, HouseName.House11, time), new[] { HouseName.House2, HouseName.House11 }, time);

        [HoroscopeCalculator(HoroscopeName.House2LordInHouse12)]
        public static CalculatorResult House2LordInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House2, HouseName.House12, time), new[] { HouseName.House2, HouseName.House12 }, time);

        [HoroscopeCalculator(HoroscopeName.House2LordInHouse12Afflicted)]
        public static CalculatorResult House2LordInHouse12Afflicted(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House2, HouseName.House12, time), new[] { HouseName.House2, HouseName.House12 }, time);

        #endregion

        #region Lord of 3rd being Situated in Different Houses

        [HoroscopeCalculator(HoroscopeName.House3LordInHouse1)]
        public static CalculatorResult House3LordInHouse1Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House3, HouseName.House1, time), new[] { HouseName.House3, HouseName.House1 }, time);

        [HoroscopeCalculator(HoroscopeName.House3LordInHouse2)]
        public static CalculatorResult House3LordInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House3, HouseName.House2, time), new[] { HouseName.House3, HouseName.House2 }, time);

        [HoroscopeCalculator(HoroscopeName.House3LordInHouse3)]
        public static CalculatorResult House3LordInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House3, HouseName.House3, time), new[] { HouseName.House3, HouseName.House3 }, time);

        [HoroscopeCalculator(HoroscopeName.House3LordInHouse4)]
        public static CalculatorResult House3LordInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House3, HouseName.House4, time), new[] { HouseName.House3, HouseName.House4 }, time);

        [HoroscopeCalculator(HoroscopeName.House3LordInHouse4Fortified)]
        public static CalculatorResult House3LordInHouse4Fortified(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House3, HouseName.House4, time), new[] { HouseName.House3, HouseName.House4 }, time);

        [HoroscopeCalculator(HoroscopeName.House3LordInHouse4WeakMars)]
        public static CalculatorResult House3LordInHouse4WeakMars(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House3, HouseName.House4, time), new[] { HouseName.House3, HouseName.House4 }, time);

        [HoroscopeCalculator(HoroscopeName.House3LordInHouse4Strong9th)]
        public static CalculatorResult House3LordInHouse4Strong9th(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House3, HouseName.House4, time), new[] { HouseName.House3, HouseName.House4 }, time);

        [HoroscopeCalculator(HoroscopeName.House3LordInHouse5)]
        public static CalculatorResult House3LordInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House3, HouseName.House5, time), new[] { HouseName.House3, HouseName.House5 }, time);

        [HoroscopeCalculator(HoroscopeName.House3LordInHouse6)]
        public static CalculatorResult House3LordInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House3, HouseName.House6, time), new[] { HouseName.House3, HouseName.House6 }, time);

        [HoroscopeCalculator(HoroscopeName.House3LordInHouse6Lord3rd)]
        public static CalculatorResult House3LordInHouse6Lord3rd(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House3, HouseName.House6, time), new[] { HouseName.House3, HouseName.House6 }, time);

        [HoroscopeCalculator(HoroscopeName.House3LordInHouse6Lord3rdAfflicted)]
        public static CalculatorResult House3LordInHouse6Lord3rdAfflicted(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House3, HouseName.House6, time), new[] { HouseName.House3, HouseName.House6 }, time);

        [HoroscopeCalculator(HoroscopeName.House3LordInHouse7)]
        public static CalculatorResult House3LordInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House3, HouseName.House7, time), new[] { HouseName.House3, HouseName.House7 }, time);

        [HoroscopeCalculator(HoroscopeName.House3LordInHouse8)]
        public static CalculatorResult House3LordInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House3, HouseName.House8, time), new[] { HouseName.House3, HouseName.House8 }, time);

        [HoroscopeCalculator(HoroscopeName.House3LordInHouse9)]
        public static CalculatorResult House3LordInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House3, HouseName.House9, time), new[] { HouseName.House3, HouseName.House9 }, time);

        [HoroscopeCalculator(HoroscopeName.House3LordInHouse10)]
        public static CalculatorResult House3LordInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House3, HouseName.House10, time), new[] { HouseName.House3, HouseName.House10 }, time);

        [HoroscopeCalculator(HoroscopeName.House3LordInHouse11)]
        public static CalculatorResult House3LordInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House3, HouseName.House11, time), new[] { HouseName.House3, HouseName.House11 }, time);

        [HoroscopeCalculator(HoroscopeName.House3LordInHouse12)]
        public static CalculatorResult House3LordInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House3, HouseName.House12, time), new[] { HouseName.House3, HouseName.House12 }, time);

        #endregion

        #region Lord of the 4th House Occupying Different Houses

        [HoroscopeCalculator(HoroscopeName.House4LordInHouse1)]
        public static CalculatorResult House4LordInHouse1Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House4, HouseName.House1, time), new[] { HouseName.House4, HouseName.House1 }, time);

        [HoroscopeCalculator(HoroscopeName.House4LordInHouse2)]
        public static CalculatorResult House4LordInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House4, HouseName.House2, time), new[] { HouseName.House4, HouseName.House2 }, time);

        [HoroscopeCalculator(HoroscopeName.House4LordInHouse3)]
        public static CalculatorResult House4LordInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House4, HouseName.House3, time), new[] { HouseName.House4, HouseName.House3 }, time);

        [HoroscopeCalculator(HoroscopeName.House4LordInHouse4)]
        public static CalculatorResult House4LordInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House4, HouseName.House4, time), new[] { HouseName.House4, HouseName.House4 }, time);

        [HoroscopeCalculator(HoroscopeName.House4LordInHouse5)]
        public static CalculatorResult House4LordInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House4, HouseName.House5, time), new[] { HouseName.House4, HouseName.House5 }, time);

        [HoroscopeCalculator(HoroscopeName.House4LordInHouse6)]
        public static CalculatorResult House4LordInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House4, HouseName.House6, time), new[] { HouseName.House4, HouseName.House6 }, time);

        [HoroscopeCalculator(HoroscopeName.House4LordInHouse7)]
        public static CalculatorResult House4LordInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House4, HouseName.House7, time), new[] { HouseName.House4, HouseName.House7 }, time);

        [HoroscopeCalculator(HoroscopeName.House4LordInHouse8)]
        public static CalculatorResult House4LordInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House4, HouseName.House8, time), new[] { HouseName.House4, HouseName.House8 }, time);

        [HoroscopeCalculator(HoroscopeName.House4LordInHouse9)]
        public static CalculatorResult House4LordInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House4, HouseName.House9, time), new[] { HouseName.House4, HouseName.House9 }, time);

        [HoroscopeCalculator(HoroscopeName.House4LordInHouse10)]
        public static CalculatorResult House4LordInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House4, HouseName.House10, time), new[] { HouseName.House4, HouseName.House10 }, time);

        [HoroscopeCalculator(HoroscopeName.House4LordInHouse10Afflicted)]
        public static CalculatorResult House4LordInHouse10Afflicted(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House4, HouseName.House10, time), new[] { HouseName.House4, HouseName.House10 }, time);

        [HoroscopeCalculator(HoroscopeName.House4LordInHouse11)]
        public static CalculatorResult House4LordInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House4, HouseName.House11, time), new[] { HouseName.House4, HouseName.House11 }, time);

        [HoroscopeCalculator(HoroscopeName.House4LordInHouse12)]
        public static CalculatorResult House4LordInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House4, HouseName.House12, time), new[] { HouseName.House4, HouseName.House12 }, time);

        #endregion

        #region Lord of the 5th House Occupying Different Houses

        [HoroscopeCalculator(HoroscopeName.House5LordInHouse1Fortified)]
        public static CalculatorResult House5LordInHouse1Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House5, HouseName.House1, time), new[] { HouseName.House5, HouseName.House1 }, time);

        [HoroscopeCalculator(HoroscopeName.House5LordInHouse1Afflicted)]
        public static CalculatorResult House5LordInHouse1Afflicted(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House5, HouseName.House1, time), new[] { HouseName.House5, HouseName.House1 }, time);

        [HoroscopeCalculator(HoroscopeName.House5LordInHouse1AfflictedModerate)]
        public static CalculatorResult House5LordInHouse1AfflictedModerate(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House5, HouseName.House1, time), new[] { HouseName.House5, HouseName.House1 }, time);

        [HoroscopeCalculator(HoroscopeName.House5LordInHouse2)]
        public static CalculatorResult House5LordInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House5, HouseName.House2, time), new[] { HouseName.House5, HouseName.House2 }, time);

        [HoroscopeCalculator(HoroscopeName.House5LordInHouse3Fortified)]
        public static CalculatorResult House5LordInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House5, HouseName.House3, time), new[] { HouseName.House5, HouseName.House3 }, time);

        [HoroscopeCalculator(HoroscopeName.House5LordInHouse3Afflicted)]
        public static CalculatorResult House5LordInHouse3Afflicted(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House5, HouseName.House3, time), new[] { HouseName.House5, HouseName.House3 }, time);

        [HoroscopeCalculator(HoroscopeName.House5LordInHouse4)]
        public static CalculatorResult House5LordInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House5, HouseName.House4, time), new[] { HouseName.House5, HouseName.House4 }, time);

        [HoroscopeCalculator(HoroscopeName.House5LordInHouse5)]
        public static CalculatorResult House5LordInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House5, HouseName.House5, time), new[] { HouseName.House5, HouseName.House5 }, time);

        [HoroscopeCalculator(HoroscopeName.House5LordInHouse6)]
        public static CalculatorResult House5LordInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House5, HouseName.House6, time), new[] { HouseName.House5, HouseName.House6 }, time);

        [HoroscopeCalculator(HoroscopeName.House5LordInHouse7)]
        public static CalculatorResult House5LordInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House5, HouseName.House7, time), new[] { HouseName.House5, HouseName.House7 }, time);

        [HoroscopeCalculator(HoroscopeName.House5LordInHouse8)]
        public static CalculatorResult House5LordInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House5, HouseName.House8, time), new[] { HouseName.House5, HouseName.House8 }, time);

        [HoroscopeCalculator(HoroscopeName.House5LordInHouse9)]
        public static CalculatorResult House5LordInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House5, HouseName.House9, time), new[] { HouseName.House5, HouseName.House9 }, time);

        [HoroscopeCalculator(HoroscopeName.House5LordInHouse9Afflicted)]
        public static CalculatorResult House5LordInHouse9Afflicted(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House5, HouseName.House9, time), new[] { HouseName.House5, HouseName.House9 }, time);

        [HoroscopeCalculator(HoroscopeName.House5LordInHouse10)]
        public static CalculatorResult House5LordInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House5, HouseName.House10, time), new[] { HouseName.House5, HouseName.House10 }, time);

        [HoroscopeCalculator(HoroscopeName.House5LordInHouse11)]
        public static CalculatorResult House5LordInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House5, HouseName.House11, time), new[] { HouseName.House5, HouseName.House11 }, time);

        [HoroscopeCalculator(HoroscopeName.House5LordInHouse12)]
        public static CalculatorResult House5LordInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House5, HouseName.House12, time), new[] { HouseName.House5, HouseName.House12 }, time);

        #endregion

        #region Lord of the 6th House Occupying Different Houses

        [HoroscopeCalculator(HoroscopeName.House6LordInHouse1Fortified)]
        public static CalculatorResult House6LordInHouse1Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House6, HouseName.House1, time), new[] { HouseName.House6, HouseName.House1 }, time);

        [HoroscopeCalculator(HoroscopeName.House6LordInHouse1Afflicted)]
        public static CalculatorResult House6LordInHouse1Afflicted(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House6, HouseName.House1, time), new[] { HouseName.House6, HouseName.House1 }, time);

        [HoroscopeCalculator(HoroscopeName.House6LordInHouse2)]
        public static CalculatorResult House6LordInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House6, HouseName.House2, time), new[] { HouseName.House6, HouseName.House2 }, time);

        [HoroscopeCalculator(HoroscopeName.House6LordInHouse3)]
        public static CalculatorResult House6LordInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House6, HouseName.House3, time), new[] { HouseName.House6, HouseName.House3 }, time);

        [HoroscopeCalculator(HoroscopeName.House6LordInHouse4)]
        public static CalculatorResult House6LordInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House6, HouseName.House4, time), new[] { HouseName.House6, HouseName.House4 }, time);

        [HoroscopeCalculator(HoroscopeName.House6LordInHouse5)]
        public static CalculatorResult House6LordInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House6, HouseName.House5, time), new[] { HouseName.House6, HouseName.House5 }, time);

        [HoroscopeCalculator(HoroscopeName.House6LordInHouse6)]
        public static CalculatorResult House6LordInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House6, HouseName.House6, time), new[] { HouseName.House6, HouseName.House6 }, time);

        [HoroscopeCalculator(HoroscopeName.House6LordInHouse7)]
        public static CalculatorResult House6LordInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House6, HouseName.House7, time), new[] { HouseName.House6, HouseName.House7 }, time);

        [HoroscopeCalculator(HoroscopeName.House6LordInHouse8)]
        public static CalculatorResult House6LordInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House6, HouseName.House8, time), new[] { HouseName.House6, HouseName.House8 }, time);

        [HoroscopeCalculator(HoroscopeName.House6LordInHouse9)]
        public static CalculatorResult House6LordInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House6, HouseName.House9, time), new[] { HouseName.House6, HouseName.House9 }, time);

        [HoroscopeCalculator(HoroscopeName.House6LordInHouse10)]
        public static CalculatorResult House6LordInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House6, HouseName.House10, time), new[] { HouseName.House6, HouseName.House10 }, time);

        [HoroscopeCalculator(HoroscopeName.House6LordInHouse11)]
        public static CalculatorResult House6LordInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House6, HouseName.House11, time), new[] { HouseName.House6, HouseName.House11 }, time);

        [HoroscopeCalculator(HoroscopeName.House6LordInHouse12)]
        public static CalculatorResult House6LordInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House6, HouseName.House12, time), new[] { HouseName.House6, HouseName.House12 }, time);

        #endregion

        #region Lord of the 7th House Occupying Different Houses

        [HoroscopeCalculator(HoroscopeName.House7LordInHouse1)]
        public static CalculatorResult House7LordInHouse1Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House7, HouseName.House1, time), new[] { HouseName.House7, HouseName.House1 }, time);
        [HoroscopeCalculator(HoroscopeName.House7LordInHouse2)]
        public static CalculatorResult House7LordInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House7, HouseName.House2, time), new[] { HouseName.House7, HouseName.House2 }, time);
        [HoroscopeCalculator(HoroscopeName.House7LordInHouse3)]
        public static CalculatorResult House7LordInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House7, HouseName.House3, time), new[] { HouseName.House7, HouseName.House3 }, time);
        [HoroscopeCalculator(HoroscopeName.House7LordInHouse4)]
        public static CalculatorResult House7LordInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House7, HouseName.House4, time), new[] { HouseName.House7, HouseName.House4 }, time);
        [HoroscopeCalculator(HoroscopeName.House7LordInHouse5)]
        public static CalculatorResult House7LordInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House7, HouseName.House5, time), new[] { HouseName.House7, HouseName.House5 }, time);
        [HoroscopeCalculator(HoroscopeName.House7LordInHouse6)]
        public static CalculatorResult House7LordInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House7, HouseName.House6, time), new[] { HouseName.House7, HouseName.House6 }, time);
        [HoroscopeCalculator(HoroscopeName.House7LordInHouse7)]
        public static CalculatorResult House7LordInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House7, HouseName.House7, time), new[] { HouseName.House7, HouseName.House7 }, time);
        [HoroscopeCalculator(HoroscopeName.House7LordInHouse8)]
        public static CalculatorResult House7LordInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House7, HouseName.House8, time), new[] { HouseName.House7, HouseName.House8 }, time);
        [HoroscopeCalculator(HoroscopeName.House7LordInHouse9)]
        public static CalculatorResult House7LordInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House7, HouseName.House9, time), new[] { HouseName.House7, HouseName.House9 }, time);
        [HoroscopeCalculator(HoroscopeName.House7LordInHouse10)]
        public static CalculatorResult House7LordInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House7, HouseName.House10, time), new[] { HouseName.House7, HouseName.House10 }, time);
        [HoroscopeCalculator(HoroscopeName.House7LordInHouse11)]
        public static CalculatorResult House7LordInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House7, HouseName.House11, time), new[] { HouseName.House7, HouseName.House11 }, time);
        [HoroscopeCalculator(HoroscopeName.House7LordInHouse12)]
        public static CalculatorResult House7LordInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House7, HouseName.House12, time), new[] { HouseName.House7, HouseName.House12 }, time);

        #endregion

        #region Lord of the 8th House Occupying Different Houses

        [HoroscopeCalculator(HoroscopeName.House8LordInHouse1)]
        public static CalculatorResult House8LordInHouse1Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House8, HouseName.House1, time), new[] { HouseName.House8, HouseName.House1 }, time);

        [HoroscopeCalculator(HoroscopeName.House8LordInHouse2)]
        public static CalculatorResult House8LordInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House8, HouseName.House2, time), new[] { HouseName.House8, HouseName.House2 }, time);

        [HoroscopeCalculator(HoroscopeName.House8LordInHouse3)]
        public static CalculatorResult House8LordInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House8, HouseName.House3, time), new[] { HouseName.House8, HouseName.House3 }, time);

        [HoroscopeCalculator(HoroscopeName.House8LordInHouse4)]
        public static CalculatorResult House8LordInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House8, HouseName.House4, time), new[] { HouseName.House8, HouseName.House4 }, time);

        [HoroscopeCalculator(HoroscopeName.House8LordInHouse5)]
        public static CalculatorResult House8LordInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House8, HouseName.House5, time), new[] { HouseName.House8, HouseName.House5 }, time);

        [HoroscopeCalculator(HoroscopeName.House8LordInHouse6)]
        public static CalculatorResult House8LordInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House8, HouseName.House6, time), new[] { HouseName.House8, HouseName.House6 }, time);

        [HoroscopeCalculator(HoroscopeName.House8LordInHouse7)]
        public static CalculatorResult House8LordInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House8, HouseName.House7, time), new[] { HouseName.House8, HouseName.House7 }, time);

        [HoroscopeCalculator(HoroscopeName.House8LordInHouse8)]
        public static CalculatorResult House8LordInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House8, HouseName.House8, time), new[] { HouseName.House8, HouseName.House8 }, time);

        [HoroscopeCalculator(HoroscopeName.House8LordInHouse9)]
        public static CalculatorResult House8LordInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House8, HouseName.House9, time), new[] { HouseName.House8, HouseName.House9 }, time);

        [HoroscopeCalculator(HoroscopeName.House8LordInHouse10)]
        public static CalculatorResult House8LordInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House8, HouseName.House10, time), new[] { HouseName.House8, HouseName.House10 }, time);

        [HoroscopeCalculator(HoroscopeName.House8LordInHouse11)]
        public static CalculatorResult House8LordInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House8, HouseName.House11, time), new[] { HouseName.House8, HouseName.House11 }, time);

        [HoroscopeCalculator(HoroscopeName.House8LordInHouse12)]
        public static CalculatorResult House8LordInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House8, HouseName.House12, time), new[] { HouseName.House8, HouseName.House12 }, time);

        #endregion

        #region Lord of the 9th House Occupying Different Houses

        [HoroscopeCalculator(HoroscopeName.House9LordInHouse1)]
        public static CalculatorResult House9LordInHouse1Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House9, HouseName.House1, time), new[] { HouseName.House9, HouseName.House1 }, time);

        [HoroscopeCalculator(HoroscopeName.House9LordInHouse2)]
        public static CalculatorResult House9LordInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House9, HouseName.House2, time), new[] { HouseName.House9, HouseName.House2 }, time);

        [HoroscopeCalculator(HoroscopeName.House9LordInHouse3)]
        public static CalculatorResult House9LordInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House9, HouseName.House3, time), new[] { HouseName.House9, HouseName.House3 }, time);

        [HoroscopeCalculator(HoroscopeName.House9LordInHouse4)]
        public static CalculatorResult House9LordInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House9, HouseName.House4, time), new[] { HouseName.House9, HouseName.House4 }, time);

        [HoroscopeCalculator(HoroscopeName.House9LordInHouse5)]
        public static CalculatorResult House9LordInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House9, HouseName.House5, time), new[] { HouseName.House9, HouseName.House5 }, time);

        [HoroscopeCalculator(HoroscopeName.House9LordInHouse6)]
        public static CalculatorResult House9LordInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House9, HouseName.House6, time), new[] { HouseName.House9, HouseName.House6 }, time);

        [HoroscopeCalculator(HoroscopeName.House9LordInHouse7)]
        public static CalculatorResult House9LordInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House9, HouseName.House7, time), new[] { HouseName.House9, HouseName.House7 }, time);

        [HoroscopeCalculator(HoroscopeName.House9LordInHouse8)]
        public static CalculatorResult House9LordInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House9, HouseName.House8, time), new[] { HouseName.House9, HouseName.House8 }, time);

        [HoroscopeCalculator(HoroscopeName.House9LordInHouse9)]
        public static CalculatorResult House9LordInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House9, HouseName.House9, time), new[] { HouseName.House9, HouseName.House9 }, time);

        [HoroscopeCalculator(HoroscopeName.House9LordInHouse10)]
        public static CalculatorResult House9LordInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House9, HouseName.House10, time), new[] { HouseName.House9, HouseName.House10 }, time);

        [HoroscopeCalculator(HoroscopeName.House9LordInHouse11)]
        public static CalculatorResult House9LordInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House9, HouseName.House11, time), new[] { HouseName.House9, HouseName.House11 }, time);

        [HoroscopeCalculator(HoroscopeName.House9LordInHouse12)]
        public static CalculatorResult House9LordInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House9, HouseName.House12, time), new[] { HouseName.House9, HouseName.House12 }, time);

        #endregion

        #region Lord of the 10th House Occupying Different Houses

        [HoroscopeCalculator(HoroscopeName.House10LordInHouse1)]
        public static CalculatorResult House10LordInHouse1Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House10, HouseName.House1, time), new[] { HouseName.House10, HouseName.House1 }, time);
        [HoroscopeCalculator(HoroscopeName.House10LordInHouse2)]
        public static CalculatorResult House10LordInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House10, HouseName.House2, time), new[] { HouseName.House10, HouseName.House2 }, time);
        [HoroscopeCalculator(HoroscopeName.House10LordInHouse3)]
        public static CalculatorResult House10LordInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House10, HouseName.House3, time), new[] { HouseName.House10, HouseName.House3 }, time);
        [HoroscopeCalculator(HoroscopeName.House10LordInHouse4)]
        public static CalculatorResult House10LordInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House10, HouseName.House4, time), new[] { HouseName.House10, HouseName.House4 }, time);
        [HoroscopeCalculator(HoroscopeName.House10LordInHouse5)]
        public static CalculatorResult House10LordInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House10, HouseName.House5, time), new[] { HouseName.House10, HouseName.House5 }, time);
        [HoroscopeCalculator(HoroscopeName.House10LordInHouse6)]
        public static CalculatorResult House10LordInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House10, HouseName.House6, time), new[] { HouseName.House10, HouseName.House6 }, time);
        [HoroscopeCalculator(HoroscopeName.House10LordInHouse7)]
        public static CalculatorResult House10LordInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House10, HouseName.House7, time), new[] { HouseName.House10, HouseName.House7 }, time);
        [HoroscopeCalculator(HoroscopeName.House10LordInHouse8)]
        public static CalculatorResult House10LordInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House10, HouseName.House8, time), new[] { HouseName.House10, HouseName.House8 }, time);
        [HoroscopeCalculator(HoroscopeName.House10LordInHouse9)]
        public static CalculatorResult House10LordInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House10, HouseName.House9, time), new[] { HouseName.House10, HouseName.House9 }, time);
        [HoroscopeCalculator(HoroscopeName.House10LordInHouse10)]
        public static CalculatorResult House10LordInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House10, HouseName.House10, time), new[] { HouseName.House10, HouseName.House10 }, time);
        [HoroscopeCalculator(HoroscopeName.House10LordInHouse11)]
        public static CalculatorResult House10LordInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House10, HouseName.House11, time), new[] { HouseName.House10, HouseName.House11 }, time);
        [HoroscopeCalculator(HoroscopeName.House10LordInHouse12)]
        public static CalculatorResult House10LordInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House10, HouseName.House12, time), new[] { HouseName.House10, HouseName.House12 }, time);


        #endregion

        #region Lord of the 11th House Occupying Different Houses

        [HoroscopeCalculator(HoroscopeName.House11LordInHouse1)]
        public static CalculatorResult House11LordInHouse1Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House11, HouseName.House1, time), new[] { HouseName.House11, HouseName.House1 }, time);

        [HoroscopeCalculator(HoroscopeName.House11LordInHouse2)]
        public static CalculatorResult House11LordInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House11, HouseName.House2, time), new[] { HouseName.House11, HouseName.House2 }, time);

        [HoroscopeCalculator(HoroscopeName.House11LordInHouse3)]
        public static CalculatorResult House11LordInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House11, HouseName.House3, time), new[] { HouseName.House11, HouseName.House3 }, time);

        [HoroscopeCalculator(HoroscopeName.House11LordInHouse4)]
        public static CalculatorResult House11LordInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House11, HouseName.House4, time), new[] { HouseName.House11, HouseName.House4 }, time);

        [HoroscopeCalculator(HoroscopeName.House11LordInHouse5)]
        public static CalculatorResult House11LordInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House11, HouseName.House5, time), new[] { HouseName.House11, HouseName.House5 }, time);

        [HoroscopeCalculator(HoroscopeName.House11LordInHouse6)]
        public static CalculatorResult House11LordInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House11, HouseName.House6, time), new[] { HouseName.House11, HouseName.House6 }, time);

        [HoroscopeCalculator(HoroscopeName.House11LordInHouse7)]
        public static CalculatorResult House11LordInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House11, HouseName.House7, time), new[] { HouseName.House11, HouseName.House7 }, time);

        [HoroscopeCalculator(HoroscopeName.House11LordInHouse8)]
        public static CalculatorResult House11LordInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House11, HouseName.House8, time), new[] { HouseName.House11, HouseName.House8 }, time);

        [HoroscopeCalculator(HoroscopeName.House11LordInHouse9)]
        public static CalculatorResult House11LordInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House11, HouseName.House9, time), new[] { HouseName.House11, HouseName.House9 }, time);

        [HoroscopeCalculator(HoroscopeName.House11LordInHouse10)]
        public static CalculatorResult House11LordInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House11, HouseName.House10, time), new[] { HouseName.House11, HouseName.House10 }, time);

        [HoroscopeCalculator(HoroscopeName.House11LordInHouse11)]
        public static CalculatorResult House11LordInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House11, HouseName.House11, time), new[] { HouseName.House11, HouseName.House11 }, time);

        [HoroscopeCalculator(HoroscopeName.House11LordInHouse12)]
        public static CalculatorResult House11LordInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House11, HouseName.House12, time), new[] { HouseName.House11, HouseName.House12 }, time);

        #endregion

        #region Lord of the 12th House Occupying Different Houses

        [HoroscopeCalculator(HoroscopeName.House12LordInHouse1)]
        public static CalculatorResult House12LordInHouse1Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House12, HouseName.House1, time), new[] { HouseName.House12, HouseName.House1 }, time);

        [HoroscopeCalculator(HoroscopeName.House12LordInHouse2)]
        public static CalculatorResult House12LordInHouse2Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House12, HouseName.House2, time), new[] { HouseName.House12, HouseName.House2 }, time);

        [HoroscopeCalculator(HoroscopeName.House12LordInHouse3)]
        public static CalculatorResult House12LordInHouse3Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House12, HouseName.House3, time), new[] { HouseName.House12, HouseName.House3 }, time);

        [HoroscopeCalculator(HoroscopeName.House12LordInHouse4)]
        public static CalculatorResult House12LordInHouse4Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House12, HouseName.House4, time), new[] { HouseName.House12, HouseName.House4 }, time);

        [HoroscopeCalculator(HoroscopeName.House12LordInHouse5)]
        public static CalculatorResult House12LordInHouse5Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House12, HouseName.House5, time), new[] { HouseName.House12, HouseName.House5 }, time);

        [HoroscopeCalculator(HoroscopeName.House12LordInHouse6)]
        public static CalculatorResult House12LordInHouse6Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House12, HouseName.House6, time), new[] { HouseName.House12, HouseName.House6 }, time);

        [HoroscopeCalculator(HoroscopeName.House12LordInHouse7)]
        public static CalculatorResult House12LordInHouse7Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House12, HouseName.House7, time), new[] { HouseName.House12, HouseName.House7 }, time);

        [HoroscopeCalculator(HoroscopeName.House12LordInHouse8)]
        public static CalculatorResult House12LordInHouse8Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House12, HouseName.House8, time), new[] { HouseName.House12, HouseName.House8 }, time);

        [HoroscopeCalculator(HoroscopeName.House12LordInHouse9)]
        public static CalculatorResult House12LordInHouse9Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House12, HouseName.House9, time), new[] { HouseName.House12, HouseName.House9 }, time);

        [HoroscopeCalculator(HoroscopeName.House12LordInHouse10)]
        public static CalculatorResult House12LordInHouse10Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House12, HouseName.House10, time), new[] { HouseName.House12, HouseName.House10 }, time);

        [HoroscopeCalculator(HoroscopeName.House12LordInHouse11)]
        public static CalculatorResult House12LordInHouse11Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House12, HouseName.House11, time), new[] { HouseName.House12, HouseName.House11 }, time);

        [HoroscopeCalculator(HoroscopeName.House12LordInHouse12)]
        public static CalculatorResult House12LordInHouse12Occuring(Time time) => CalculatorResult.New(Calculate.IsHouseLordInHouse(HouseName.House12, HouseName.House12, time), new[] { HouseName.House12, HouseName.House12 }, time);

        #endregion


        #endregion

        #region DIFFERENT SIGNS ASCENDING

        [HoroscopeCalculator(HoroscopeName.AriesRising)]
        public static CalculatorResult AriesRisingOccuring(Time time) => CalculatorResult.New(Calculate.IsHouseSignName(HouseName.House1, ZodiacName.Aries, time), new[] { HouseName.House1, }, new[] { ZodiacName.Aries }, time);

        [HoroscopeCalculator(HoroscopeName.TaurusRising)]
        public static CalculatorResult TaurusRisingOccuring(Time time) => CalculatorResult.New(Calculate.IsHouseSignName(HouseName.House1, ZodiacName.Taurus, time), new[] { HouseName.House1, }, new[] { ZodiacName.Taurus }, time);

        [HoroscopeCalculator(HoroscopeName.GeminiRising)]
        public static CalculatorResult GeminiRisingOccuring(Time time) => CalculatorResult.New(Calculate.IsHouseSignName(HouseName.House1, ZodiacName.Gemini, time), new[] { HouseName.House1, }, new[] { ZodiacName.Gemini }, time);

        [HoroscopeCalculator(HoroscopeName.CancerRising)]
        public static CalculatorResult CancerRisingOccuring(Time time) => CalculatorResult.New(Calculate.IsHouseSignName(HouseName.House1, ZodiacName.Cancer, time), new[] { HouseName.House1, }, new[] { ZodiacName.Cancer }, time);

        [HoroscopeCalculator(HoroscopeName.LeoRising)]
        public static CalculatorResult LeoRisingOccuring(Time time) => CalculatorResult.New(Calculate.IsHouseSignName(HouseName.House1, ZodiacName.Leo, time), new[] { HouseName.House1, }, new[] { ZodiacName.Leo }, time);

        [HoroscopeCalculator(HoroscopeName.VirgoRising)]
        public static CalculatorResult VirgoRisingOccuring(Time time) => CalculatorResult.New(Calculate.IsHouseSignName(HouseName.House1, ZodiacName.Virgo, time), new[] { HouseName.House1, }, new[] { ZodiacName.Virgo }, time);

        [HoroscopeCalculator(HoroscopeName.LibraRising)]
        public static CalculatorResult LibraRisingOccuring(Time time) => CalculatorResult.New(Calculate.IsHouseSignName(HouseName.House1, ZodiacName.Libra, time), new[] { HouseName.House1, }, new[] { ZodiacName.Libra }, time);

        [HoroscopeCalculator(HoroscopeName.ScorpioRising)]
        public static CalculatorResult ScorpioRisingOccuring(Time time) => CalculatorResult.New(Calculate.IsHouseSignName(HouseName.House1, ZodiacName.Scorpio, time), new[] { HouseName.House1, }, new[] { ZodiacName.Scorpio }, time);

        [HoroscopeCalculator(HoroscopeName.SagittariusRising)]
        public static CalculatorResult SagittariusRisingOccuring(Time time) => CalculatorResult.New(Calculate.IsHouseSignName(HouseName.House1, ZodiacName.Sagittarius, time), new[] { HouseName.House1, }, new[] { ZodiacName.Sagittarius }, time);

        [HoroscopeCalculator(HoroscopeName.CapricornRising)]
        public static CalculatorResult CapricornRisingOccuring(Time time) => CalculatorResult.New(Calculate.IsHouseSignName(HouseName.House1, ZodiacName.Capricorn, time), new[] { HouseName.House1, }, new[] { ZodiacName.Capricorn }, time);

        [HoroscopeCalculator(HoroscopeName.AquariusRising)]
        public static CalculatorResult AquariusRisingOccuring(Time time) => CalculatorResult.New(Calculate.IsHouseSignName(HouseName.House1, ZodiacName.Aquarius, time), new[] { HouseName.House1, }, new[] { ZodiacName.Aquarius }, time);

        [HoroscopeCalculator(HoroscopeName.PiscesRising)]
        public static CalculatorResult PiscesRisingOccuring(Time time) => CalculatorResult.New(Calculate.IsHouseSignName(HouseName.House1, ZodiacName.Pisces, time), new[] { HouseName.House1, }, new[] { ZodiacName.Pisces }, time);

        #endregion

        #region 2ND HOUSE SPECIAL COMBINATIONS

        [HoroscopeCalculator(HoroscopeName.Lord2WithEvilInHouse)]
        public static CalculatorResult Lord2WithEvilInHouse(Time time)
        {
            //If the 2nd lord is in the 2nd with(1) evil planets or aspected by him(2), he will be poor.
            //NOTE: 1."with" here is interpreted as same house
            //      2. interpreted as evil planets transmitting aspect to 2nd lord (receiving aspect)
            //TODO check validity


            //if 2nd lord not in second, end here
            var lord = Calculate.LordOfHouse(HouseName.House2, time);
            var lordPlace = Calculate.HousePlanetOccupies(lord, time);
            if (lordPlace != HouseName.House2) { return CalculatorResult.NotOccuring(); }

            //evil planet in house 2, prediction occuring
            var evilInHouse2 = Calculate.IsMaleficPlanetInHouse(HouseName.House2, time);

            //if evil planets aspect the lord, prediction occuring
            var aspectedByEvil = Calculate.IsPlanetAspectedByMaleficPlanets(lord, time);

            //either one true for prediction to occur
            var occurring = evilInHouse2 || aspectedByEvil;

            return CalculatorResult.New(occurring, lord);
        }

        [HoroscopeCalculator(HoroscopeName.SaturnIn2WithVenus)]
        public static CalculatorResult SaturnIn2WithVenus(Time time)
        {
            //Ordinary wealth is indicated if Saturn is in the 2nd aspected by Venus.

            //if saturn not in 2nd end here
            var saturnHouse = Calculate.HousePlanetOccupies(PlanetName.Saturn, time);
            var saturnIn2 = saturnHouse == HouseName.House2;
            if (!saturnIn2) { return CalculatorResult.NotOccuring(); }

            //if venus is aspecting saturn, event occuring
            var venusAspecting =
                Calculate.IsPlanetAspectedByPlanet(PlanetName.Saturn, PlanetName.Venus, time);

            return CalculatorResult.New(venusAspecting, new[] { HouseName.House2 }, new[] { PlanetName.Saturn, PlanetName.Venus }, time);
        }

        [HoroscopeCalculator(HoroscopeName.MoonMarsIn2WithSaturnAspect)]
        public static CalculatorResult MoonMarsIn2WithSaturnAspect(Time time)
        {
            //If the Moon and Mars reside in the 2nd bhava and Saturn aspects it,
            //he suffers from a peculiar skin disease.

            //moon and mars in 2nd
            var moonIn2 = Calculate.HousePlanetOccupies(PlanetName.Moon, time) == HouseName.House2;
            var marsIn2 = Calculate.HousePlanetOccupies(PlanetName.Mars, time) == HouseName.House2;

            //saturn aspects 2nd House
            var saturnAspects2nd =
                Calculate.IsHouseAspectedByPlanet(HouseName.House2, PlanetName.Saturn, time);

            //check if all conditions met
            var occuring = moonIn2 && marsIn2 && saturnAspects2nd;

            return CalculatorResult.New(occuring, new[] { HouseName.House2 }, new[] { PlanetName.Moon, PlanetName.Mars, PlanetName.Saturn }, time);
        }

        [HoroscopeCalculator(HoroscopeName.MercuryAndEvilIn2WithMoonAspect)]
        public static CalculatorResult MercuryAndEvilIn2WithMoonAspect(Time time)
        {
            //The situation of Mercury in the 2nd with another evil planet aspected by the Moon is bad for saving money.
            //Even if there is any ancestral wealth, it will be spent—rather wasted on extravagant purposes.

            //is mercury in 2nd house
            var mercuryIn2 = Calculate.HousePlanetOccupies(PlanetName.Mercury, time) == HouseName.House2;

            //evil planet in 2nd house
            var evilPlanetIn2 = Calculate.IsMaleficPlanetInHouse(HouseName.House2, time);

            //moon aspects 2nd House
            var moonAspects2nd =
                Calculate.IsHouseAspectedByPlanet(HouseName.House2, PlanetName.Moon, time);

            //check if all conditions met
            var occuring = mercuryIn2 && evilPlanetIn2 && moonAspects2nd;

            return CalculatorResult.New(occuring, new[] { HouseName.House2 }, new[] { PlanetName.Moon, PlanetName.Mercury }, time);
        }

        [HoroscopeCalculator(HoroscopeName.SunIn2WithNoSaturnAspect)]
        public static CalculatorResult SunIn2WithNoSaturnAspect(Time time)
        {
            //The Sun in the 2nd without being aspected by Saturn is favourable for a steady fortune.

            //sun in 2nd
            var sunIn2 = Calculate.HousePlanetOccupies(PlanetName.Sun, time) == HouseName.House2;

            //saturn aspects 2nd House
            var saturnNotAspects2nd = !Calculate.IsHouseAspectedByPlanet(HouseName.House2, PlanetName.Saturn, time);

            //check if all conditions met
            var occuring = sunIn2 && saturnNotAspects2nd;

            return CalculatorResult.New(occuring, new[] { HouseName.House2 }, new[] { PlanetName.Sun }, time);
        }

        [HoroscopeCalculator(HoroscopeName.MoonIn2WithMercuryAspect)]
        public static CalculatorResult MoonIn2WithMercuryAspect(Time time)
        {
            //The Moon being placed in the 2nd and aspected by Mercury is favourable for earning money by self-exertion.

            //moon in 2nd
            var moonIn2 = Calculate.HousePlanetOccupies(PlanetName.Moon, time) == HouseName.House2;

            //mercury aspects 2nd House
            var mercuryAspects2nd =
                Calculate.IsHouseAspectedByPlanet(HouseName.House2, PlanetName.Mercury, time);

            //check if all conditions met
            var occuring = moonIn2 && mercuryAspects2nd;

            return CalculatorResult.New(occuring, new[] { HouseName.House2 }, new[] { PlanetName.Moon, PlanetName.Mercury }, time);
        }

        [HoroscopeCalculator(HoroscopeName.Lord2And3In6WithEvilPlanet)]
        public static CalculatorResult Lord2And3In6WithEvilPlanet(Time time)
        {
            //He will be poor if lords of the 2nd and 3rd are in the 6th with or aspected by evil planets.

            //lord 2 in 6th
            var lord2 = Calculate.LordOfHouse(HouseName.House2, time);
            var lord2In6 = Calculate.HousePlanetOccupies(lord2, time) == HouseName.House6;

            //lord 3 in 6th
            var lord3 = Calculate.LordOfHouse(HouseName.House3, time);
            var lord3In6 = Calculate.HousePlanetOccupies(lord3, time) == HouseName.House6;

            //evil planets in 6th house OR aspecting the 6th
            var evilPlanetIn6 = Calculate.IsMaleficPlanetInHouse(HouseName.House6, time);
            var evilPlanetAspects6 = Calculate.IsMaleficPlanetAspectHouse(HouseName.House6, time);
            var evilPresentIn6 = evilPlanetIn6 || evilPlanetAspects6;

            //check if all conditions met
            var occuring = lord2In6 && lord3In6 && evilPresentIn6;

            return CalculatorResult.New(occuring, new[] { HouseName.House2, HouseName.House3, HouseName.House6 }, new[] { lord2, lord3 }, time);
        }

        [HoroscopeCalculator(HoroscopeName.Lord2InHouse1)]
        public static CalculatorResult Lord2InHouse1(Time time)
        {
            //If the second lord is in the first — One earns money by his own exertions and generally by manual labour.

            //lord 2 in house 1
            var lord2 = Calculate.LordOfHouse(HouseName.House2, time);
            var lord2In1 = Calculate.HousePlanetOccupies(lord2, time) == HouseName.House1;

            //check if all conditions met
            var occuring = lord2In1;

            return CalculatorResult.New(occuring, new[] { HouseName.House1 }, new[] { lord2 }, time);
        }

        [HoroscopeCalculator(HoroscopeName.Lord2InHouse1AndLord1InHouse2)]
        public static CalculatorResult Lord2InHouse1AndLord1InHouse2(Time time)
        {
            //In the second — Riches will be acquired without effort if the 1st and 2nd lords have exchanged their houses.
            //Note: Prediction is part of positions of lord 2 in varies houses,
            //      but for lord 2 in house 2, this "exchange" is mentioned.
            //      Further checking needed.

            //lord 1 in house 2
            var lord1 = Calculate.LordOfHouse(HouseName.House1, time);
            var lord1In2 = Calculate.HousePlanetOccupies(lord1, time) == HouseName.House2;

            //lord 2 in house 1
            var lord2 = Calculate.LordOfHouse(HouseName.House2, time);
            var lord2In1 = Calculate.HousePlanetOccupies(lord2, time) == HouseName.House1;

            //check if all conditions met
            var occuring = lord2In1 && lord1In2;

            return CalculatorResult.New(occuring, new[] { HouseName.House1, HouseName.House2 }, new[] { lord1, lord2 }, time);
        }

        [HoroscopeCalculator(HoroscopeName.Lord2InHouse3)]
        public static CalculatorResult Lord2InHouse3(Time time)
        {
            //In the third — Loss from relatives, brothers and gain from travels and journeys.

            //lord 2 in house 3
            var lord2 = Calculate.LordOfHouse(HouseName.House2, time);
            var lord2In3 = Calculate.HousePlanetOccupies(lord2, time) == HouseName.House3;

            //check if all conditions met
            var occuring = lord2In3;

            return CalculatorResult.New(occuring, new[] { HouseName.House3 }, new[] { lord2 }, time);
        }

        [HoroscopeCalculator(HoroscopeName.Lord2InHouse4)]
        public static CalculatorResult Lord2InHouse4(Time time)
        {
            //In the fourth - Through mother, inheritance.

            //lord 2 in house 4
            var lord2 = Calculate.LordOfHouse(HouseName.House2, time);
            var lord2In4 = Calculate.HousePlanetOccupies(lord2, time) == HouseName.House4;

            //check if all conditions met
            var occuring = lord2In4;

            return CalculatorResult.New(occuring, new[] { HouseName.House4 }, new[] { lord2 }, time);
        }

        [HoroscopeCalculator(HoroscopeName.Lord2InHouse5)]
        public static CalculatorResult Lord2InHouse5(Time time)
        {
            //In the fifth — Ancestral properties, speculation and chance games.

            //lord 2 in house 5
            var lord2 = Calculate.LordOfHouse(HouseName.House2, time);
            var lord2In5 = Calculate.HousePlanetOccupies(lord2, time) == HouseName.House5;

            //check if all conditions met
            var occuring = lord2In5;

            return CalculatorResult.New(occuring, new[] { HouseName.House5 }, new[] { lord2 }, time);
        }

        [HoroscopeCalculator(HoroscopeName.Lord2InHouse6)]
        public static CalculatorResult Lord2InHouse6(Time time)
        {
            //In the sixth — Broker's business, loss from relatives.

            //lord 2 in house 6
            var lord2 = Calculate.LordOfHouse(HouseName.House2, time);
            var lord2In6 = Calculate.HousePlanetOccupies(lord2, time) == HouseName.House6;

            //check if all conditions met
            var occuring = lord2In6;

            return CalculatorResult.New(occuring, new[] { HouseName.House6 }, new[] { lord2 }, time);
        }

        [HoroscopeCalculator(HoroscopeName.Lord2InHouse7)]
        public static CalculatorResult Lord2InHouse7(Time time)
        {
            //In the seventh — Gain after marriage but loss from sickness, etc., of wife.

            //lord 2 in house 7
            var lord2 = Calculate.LordOfHouse(HouseName.House2, time);
            var lord2In7 = Calculate.HousePlanetOccupies(lord2, time) == HouseName.House7;

            //check if all conditions met
            var occuring = lord2In7;

            return CalculatorResult.New(occuring, new[] { HouseName.House7 }, new[] { lord2 }, time);
        }

        [HoroscopeCalculator(HoroscopeName.Lord2InHouse8)]
        public static CalculatorResult Lord2InHouse8(Time time)
        {
            //In the eighth — Legacies and enemies (source of income).

            //lord 2 in house 8
            var lord2 = Calculate.LordOfHouse(HouseName.House2, time);
            var lord2In8 = Calculate.HousePlanetOccupies(lord2, time) == HouseName.House8;

            //check if all conditions met
            var occuring = lord2In8;

            return CalculatorResult.New(occuring, new[] { HouseName.House8 }, new[] { lord2 }, time);
        }

        [HoroscopeCalculator(HoroscopeName.Lord2InHouse9)]
        public static CalculatorResult Lord2InHouse9(Time time)
        {
            //In the ninth — From father, voyages and shipping.

            //lord 2 in house 9
            var lord2 = Calculate.LordOfHouse(HouseName.House2, time);
            var lord2In9 = Calculate.HousePlanetOccupies(lord2, time) == HouseName.House9;

            //check if all conditions met
            var occuring = lord2In9;

            var info = $"Lord 2:{lord2}";
            return CalculatorResult.New(occuring, new[] { HouseName.House9 }, new[] { lord2 }, time);
        }

        [HoroscopeCalculator(HoroscopeName.Lord2InHouse10)]
        public static CalculatorResult Lord2InHouse10(Time time)
        {
            //In the tenth — Profession, eminent people, government favours.

            //lord 2 in house 10
            var lord2 = Calculate.LordOfHouse(HouseName.House2, time);
            var lord2In10 = Calculate.HousePlanetOccupies(lord2, time) == HouseName.House10;

            //check if all conditions met
            var occuring = lord2In10;

            return CalculatorResult.New(occuring, new[] { HouseName.House10 }, new[] { lord2 }, time);
        }

        [HoroscopeCalculator(HoroscopeName.Lord2InHouse11)]
        public static CalculatorResult Lord2InHouse11(Time time)
        {
            //In the eleventh — From different means.

            //lord 2 in house 11
            var lord2 = Calculate.LordOfHouse(HouseName.House2, time);
            var lord2In11 = Calculate.HousePlanetOccupies(lord2, time) == HouseName.House11;

            //check if all conditions met
            var occuring = lord2In11;

            return CalculatorResult.New(occuring, new[] { HouseName.House11 }, new[] { lord2 }, time);
        }

        [HoroscopeCalculator(HoroscopeName.Lord2InHouse12)]
        public static CalculatorResult Lord2InHouse12(Time time)
        {
            //In the twelfth — Gain from servants and unscrupulous means including illegal gratifications.

            //lord 2 in house 12
            var lord2 = Calculate.LordOfHouse(HouseName.House2, time);
            var lord2In12 = Calculate.HousePlanetOccupies(lord2, time) == HouseName.House12;

            //check if all conditions met
            var occuring = lord2In12;

            var info = $"Lord 2:{lord2}";
            return CalculatorResult.New(occuring, new[] { HouseName.House12 }, new[] { lord2 }, time);
        }

        [HoroscopeCalculator(HoroscopeName.MaleficIn11FromArudha)]
        public static CalculatorResult MaleficIn11FromArudha(Time time)
        {
            //The just or unjust means of earning depends upon the presence of
            //benefic or malefic planets in the 11th from Arudha Lagna.

            //Note : here only malefic is checked, if benefic are present than not accounted for
            //      it is gussed that results would be mixed, needs further confirmation


            //get Arudha Lagna
            var arudhaLagna = Calculate.ArudhaLagnaSign(time);

            //get 11th sign from Arudha lagna
            var sign11fromArudha = Calculate.SignCountedFromInputSign(arudhaLagna, 11);

            //see if malefic planets are in that sign
            var maleficFound = Calculate.IsMaleficPlanetInSign(sign11fromArudha, time);

            //check if all conditions met
            var occuring = maleficFound;

            var malefics = Calculate.MaleficPlanetListInSign(sign11fromArudha, time);

            return CalculatorResult.New(occuring, new[] { HouseName.House11 }, malefics.ToArray(), time);
        }

        [HoroscopeCalculator(HoroscopeName.BeneficIn11FromArudha)]
        public static CalculatorResult BeneficIn11FromArudha(Time time)
        {
            //The just or unjust means of earning depends upon the presence of
            //benefic or malefic planets in the 11th from Arudha Lagna.

            //Note : here only benefic is checked, if malefic are present than not accounted for
            //      it is gussed that results would be mixed, needs further confirmation


            //get Arudha Lagna
            var arudhaLagna = Calculate.ArudhaLagnaSign(time);

            //get 11th sign from Arudha lagna
            var sign11fromArudha = Calculate.SignCountedFromInputSign(arudhaLagna, 11);

            //see if benefic planets are in that sign
            var beneficFound = Calculate.IsBeneficPlanetInSign(sign11fromArudha, time);

            //check if all conditions met
            var occuring = beneficFound;

            var benefics = Calculate.BeneficPlanetListInSign(sign11fromArudha, time);

            return CalculatorResult.New(occuring, new[] { HouseName.House11 }, benefics.ToArray(), time);
        }





        //TODO 
        //If the lord of the 2nd is Jupiter or Jupiter resides unaspected by malefics, there will be much wealth.

        //If the lord of the 2nd is Jupiter or Jupiter resides unaspected by malefics, there will be much wealth.
        //He loses wealth if Mercury (aspected by the Moon) contacts this combination.

        //If lords of the 2nd and 11th interchange their places(1) or both are in kendras or quadrants and one aspected
        //or joined by Mercury or Jupiter, the person will be pretty rich.

        //One will always be indigent if lords of the 2nd and 11th remain separate without evil planets or aspected by them.

        //Money will be spent on moral purposes when Jupiter is in the 11th house, Venus in the 2nd and its lord with benefics.

        //If the 2nd lord is with good planets in a kendra or if the 2nd house has all the good
        //association and aspects he will be on good terms with relatives.

        //One becomes a good mathematician if Mars is in the 2nd with the Moon or aspected by Mercury. The same result can be
        //foretold if Jupiter is in the ascendant and Saturn in the 8th or if Jupiter is in a quadrant and the lord of Lagna or Mercury is exalted.

        //The person will be an able debator if the Sun or the Moon is aspected by Jupiter or Venus.


        #endregion

        #region PLANETS IN SIGN

        //SUN
        [HoroscopeCalculator(HoroscopeName.SunInAries)]
        public static CalculatorResult SunInAries(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Sun, ZodiacName.Aries, time), new[] { PlanetName.Sun }, new[] { ZodiacName.Aries }, time);
        [HoroscopeCalculator(HoroscopeName.SunInTaurus)]
        public static CalculatorResult SunInTaurus(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Sun, ZodiacName.Taurus, time), new[] { PlanetName.Sun }, new[] { ZodiacName.Taurus }, time);
        [HoroscopeCalculator(HoroscopeName.SunInGemini)]
        public static CalculatorResult SunInGemini(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Sun, ZodiacName.Gemini, time), new[] { PlanetName.Sun }, new[] { ZodiacName.Gemini }, time);
        [HoroscopeCalculator(HoroscopeName.SunInCancer)]
        public static CalculatorResult SunInCancer(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Sun, ZodiacName.Cancer, time), new[] { PlanetName.Sun }, new[] { ZodiacName.Cancer }, time);
        [HoroscopeCalculator(HoroscopeName.SunInLeo)]
        public static CalculatorResult SunInLeo(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Sun, ZodiacName.Leo, time), new[] { PlanetName.Sun }, new[] { ZodiacName.Leo }, time);
        [HoroscopeCalculator(HoroscopeName.SunInVirgo)]
        public static CalculatorResult SunInVirgo(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Sun, ZodiacName.Virgo, time), new[] { PlanetName.Sun }, new[] { ZodiacName.Virgo }, time);
        [HoroscopeCalculator(HoroscopeName.SunInLibra)]
        public static CalculatorResult SunInLibra(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Sun, ZodiacName.Libra, time), new[] { PlanetName.Sun }, new[] { ZodiacName.Libra }, time);
        [HoroscopeCalculator(HoroscopeName.SunInScorpio)]
        public static CalculatorResult SunInScorpio(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Sun, ZodiacName.Scorpio, time), new[] { PlanetName.Sun }, new[] { ZodiacName.Scorpio }, time);
        [HoroscopeCalculator(HoroscopeName.SunInSagittarius)]
        public static CalculatorResult SunInSagittarius(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Sun, ZodiacName.Sagittarius, time), new[] { PlanetName.Sun }, new[] { ZodiacName.Sagittarius }, time);
        [HoroscopeCalculator(HoroscopeName.SunInCapricorn)]
        public static CalculatorResult SunInCapricorn(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Sun, ZodiacName.Capricorn, time), new[] { PlanetName.Sun }, new[] { ZodiacName.Capricorn }, time);
        [HoroscopeCalculator(HoroscopeName.SunInAquarius)]
        public static CalculatorResult SunInAquarius(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Sun, ZodiacName.Aquarius, time), new[] { PlanetName.Sun }, new[] { ZodiacName.Aquarius }, time);
        [HoroscopeCalculator(HoroscopeName.SunInPisces)]
        public static CalculatorResult SunInPisces(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Sun, ZodiacName.Pisces, time), new[] { PlanetName.Sun }, new[] { ZodiacName.Pisces }, time);

        //MOON
        [HoroscopeCalculator(HoroscopeName.MoonInAries)]
        public static CalculatorResult MoonInAries(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Moon, ZodiacName.Aries, time), new[] { PlanetName.Moon }, new[] { ZodiacName.Aries }, time);
        [HoroscopeCalculator(HoroscopeName.MoonInTaurus)]
        public static CalculatorResult MoonInTaurus(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Moon, ZodiacName.Taurus, time), new[] { PlanetName.Moon }, new[] { ZodiacName.Taurus }, time);
        [HoroscopeCalculator(HoroscopeName.MoonInGemini)]
        public static CalculatorResult MoonInGemini(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Moon, ZodiacName.Gemini, time), new[] { PlanetName.Moon }, new[] { ZodiacName.Gemini }, time);
        [HoroscopeCalculator(HoroscopeName.MoonInCancer)]
        public static CalculatorResult MoonInCancer(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Moon, ZodiacName.Cancer, time), new[] { PlanetName.Moon }, new[] { ZodiacName.Cancer }, time);
        [HoroscopeCalculator(HoroscopeName.MoonInLeo)]
        public static CalculatorResult MoonInLeo(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Moon, ZodiacName.Leo, time), new[] { PlanetName.Moon }, new[] { ZodiacName.Leo }, time);
        [HoroscopeCalculator(HoroscopeName.MoonInVirgo)]
        public static CalculatorResult MoonInVirgo(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Moon, ZodiacName.Virgo, time), new[] { PlanetName.Moon }, new[] { ZodiacName.Virgo }, time);
        [HoroscopeCalculator(HoroscopeName.MoonInLibra)]
        public static CalculatorResult MoonInLibra(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Moon, ZodiacName.Libra, time), new[] { PlanetName.Moon }, new[] { ZodiacName.Libra }, time);
        [HoroscopeCalculator(HoroscopeName.MoonInScorpio)]
        public static CalculatorResult MoonInScorpio(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Moon, ZodiacName.Scorpio, time), new[] { PlanetName.Moon }, new[] { ZodiacName.Scorpio }, time);
        [HoroscopeCalculator(HoroscopeName.MoonInSagittarius)]
        public static CalculatorResult MoonInSagittarius(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Moon, ZodiacName.Sagittarius, time), new[] { PlanetName.Moon }, new[] { ZodiacName.Sagittarius }, time);
        [HoroscopeCalculator(HoroscopeName.MoonInCapricorn)]
        public static CalculatorResult MoonInCapricorn(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Moon, ZodiacName.Capricorn, time), new[] { PlanetName.Moon }, new[] { ZodiacName.Capricorn }, time);
        [HoroscopeCalculator(HoroscopeName.MoonInAquarius)]
        public static CalculatorResult MoonInAquarius(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Moon, ZodiacName.Aquarius, time), new[] { PlanetName.Moon }, new[] { ZodiacName.Aquarius }, time);
        [HoroscopeCalculator(HoroscopeName.MoonInPisces)]
        public static CalculatorResult MoonInPisces(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Moon, ZodiacName.Pisces, time), new[] { PlanetName.Moon }, new[] { ZodiacName.Pisces }, time);


        //MARS
        [HoroscopeCalculator(HoroscopeName.MarsInAries)]
        public static CalculatorResult MarsInAries(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mars, ZodiacName.Aries, time), new[] { PlanetName.Mars }, new[] { ZodiacName.Aries }, time);
        [HoroscopeCalculator(HoroscopeName.MarsInTaurus)]
        public static CalculatorResult MarsInTaurus(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mars, ZodiacName.Taurus, time), new[] { PlanetName.Mars }, new[] { ZodiacName.Taurus }, time);
        [HoroscopeCalculator(HoroscopeName.MarsInGemini)]
        public static CalculatorResult MarsInGemini(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mars, ZodiacName.Gemini, time), new[] { PlanetName.Mars }, new[] { ZodiacName.Gemini }, time);
        [HoroscopeCalculator(HoroscopeName.MarsInCancer)]
        public static CalculatorResult MarsInCancer(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mars, ZodiacName.Cancer, time), new[] { PlanetName.Mars }, new[] { ZodiacName.Cancer }, time);
        [HoroscopeCalculator(HoroscopeName.MarsInLeo)]
        public static CalculatorResult MarsInLeo(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mars, ZodiacName.Leo, time), new[] { PlanetName.Mars }, new[] { ZodiacName.Leo }, time);
        [HoroscopeCalculator(HoroscopeName.MarsInVirgo)]
        public static CalculatorResult MarsInVirgo(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mars, ZodiacName.Virgo, time), new[] { PlanetName.Mars }, new[] { ZodiacName.Virgo }, time);
        [HoroscopeCalculator(HoroscopeName.MarsInLibra)]
        public static CalculatorResult MarsInLibra(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mars, ZodiacName.Libra, time), new[] { PlanetName.Mars }, new[] { ZodiacName.Libra }, time);
        [HoroscopeCalculator(HoroscopeName.MarsInScorpio)]
        public static CalculatorResult MarsInScorpio(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mars, ZodiacName.Scorpio, time), new[] { PlanetName.Mars }, new[] { ZodiacName.Scorpio }, time);
        [HoroscopeCalculator(HoroscopeName.MarsInSagittarius)]
        public static CalculatorResult MarsInSagittarius(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mars, ZodiacName.Sagittarius, time), new[] { PlanetName.Mars }, new[] { ZodiacName.Sagittarius }, time);
        [HoroscopeCalculator(HoroscopeName.MarsInCapricorn)]
        public static CalculatorResult MarsInCapricorn(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mars, ZodiacName.Capricorn, time), new[] { PlanetName.Mars }, new[] { ZodiacName.Capricorn }, time);
        [HoroscopeCalculator(HoroscopeName.MarsInAquarius)]
        public static CalculatorResult MarsInAquarius(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mars, ZodiacName.Aquarius, time), new[] { PlanetName.Mars }, new[] { ZodiacName.Aquarius }, time);
        [HoroscopeCalculator(HoroscopeName.MarsInPisces)]
        public static CalculatorResult MarsInPisces(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mars, ZodiacName.Pisces, time), new[] { PlanetName.Mars }, new[] { ZodiacName.Pisces }, time);


        //MERCURY
        [HoroscopeCalculator(HoroscopeName.MercuryInAries)]
        public static CalculatorResult MercuryInAries(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mercury, ZodiacName.Aries, time), new[] { PlanetName.Mercury }, new[] { ZodiacName.Aries }, time);
        [HoroscopeCalculator(HoroscopeName.MercuryInTaurus)]
        public static CalculatorResult MercuryInTaurus(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mercury, ZodiacName.Taurus, time), new[] { PlanetName.Mercury }, new[] { ZodiacName.Taurus }, time);
        [HoroscopeCalculator(HoroscopeName.MercuryInGemini)]
        public static CalculatorResult MercuryInGemini(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mercury, ZodiacName.Gemini, time), new[] { PlanetName.Mercury }, new[] { ZodiacName.Gemini }, time);
        [HoroscopeCalculator(HoroscopeName.MercuryInCancer)]
        public static CalculatorResult MercuryInCancer(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mercury, ZodiacName.Cancer, time), new[] { PlanetName.Mercury }, new[] { ZodiacName.Cancer }, time);
        [HoroscopeCalculator(HoroscopeName.MercuryInLeo)]
        public static CalculatorResult MercuryInLeo(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mercury, ZodiacName.Leo, time), new[] { PlanetName.Mercury }, new[] { ZodiacName.Leo }, time);
        [HoroscopeCalculator(HoroscopeName.MercuryInVirgo)]
        public static CalculatorResult MercuryInVirgo(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mercury, ZodiacName.Virgo, time), new[] { PlanetName.Mercury }, new[] { ZodiacName.Virgo }, time);
        [HoroscopeCalculator(HoroscopeName.MercuryInLibra)]
        public static CalculatorResult MercuryInLibra(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mercury, ZodiacName.Libra, time), new[] { PlanetName.Mercury }, new[] { ZodiacName.Libra }, time);
        [HoroscopeCalculator(HoroscopeName.MercuryInScorpio)]
        public static CalculatorResult MercuryInScorpio(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mercury, ZodiacName.Scorpio, time), new[] { PlanetName.Mercury }, new[] { ZodiacName.Scorpio }, time);
        [HoroscopeCalculator(HoroscopeName.MercuryInSagittarius)]
        public static CalculatorResult MercuryInSagittarius(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mercury, ZodiacName.Sagittarius, time), new[] { PlanetName.Mercury }, new[] { ZodiacName.Sagittarius }, time);
        [HoroscopeCalculator(HoroscopeName.MercuryInCapricorn)]
        public static CalculatorResult MercuryInCapricorn(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mercury, ZodiacName.Capricorn, time), new[] { PlanetName.Mercury }, new[] { ZodiacName.Capricorn }, time);
        [HoroscopeCalculator(HoroscopeName.MercuryInAquarius)]
        public static CalculatorResult MercuryInAquarius(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mercury, ZodiacName.Aquarius, time), new[] { PlanetName.Mercury }, new[] { ZodiacName.Aquarius }, time);
        [HoroscopeCalculator(HoroscopeName.MercuryInPisces)]
        public static CalculatorResult MercuryInPisces(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Mercury, ZodiacName.Pisces, time), new[] { PlanetName.Mercury }, new[] { ZodiacName.Pisces }, time);

        //JUPITER
        [HoroscopeCalculator(HoroscopeName.JupiterInAries)]
        public static CalculatorResult JupiterInAries(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Jupiter, ZodiacName.Aries, time), new[] { PlanetName.Jupiter }, new[] { ZodiacName.Aries }, time);
        [HoroscopeCalculator(HoroscopeName.JupiterInTaurus)]
        public static CalculatorResult JupiterInTaurus(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Jupiter, ZodiacName.Taurus, time), new[] { PlanetName.Jupiter }, new[] { ZodiacName.Taurus }, time);
        [HoroscopeCalculator(HoroscopeName.JupiterInGemini)]
        public static CalculatorResult JupiterInGemini(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Jupiter, ZodiacName.Gemini, time), new[] { PlanetName.Jupiter }, new[] { ZodiacName.Gemini }, time);
        [HoroscopeCalculator(HoroscopeName.JupiterInCancer)]
        public static CalculatorResult JupiterInCancer(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Jupiter, ZodiacName.Cancer, time), new[] { PlanetName.Jupiter }, new[] { ZodiacName.Cancer }, time);
        [HoroscopeCalculator(HoroscopeName.JupiterInLeo)]
        public static CalculatorResult JupiterInLeo(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Jupiter, ZodiacName.Leo, time), new[] { PlanetName.Jupiter }, new[] { ZodiacName.Leo }, time);
        [HoroscopeCalculator(HoroscopeName.JupiterInVirgo)]
        public static CalculatorResult JupiterInVirgo(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Jupiter, ZodiacName.Virgo, time), new[] { PlanetName.Jupiter }, new[] { ZodiacName.Virgo }, time);
        [HoroscopeCalculator(HoroscopeName.JupiterInLibra)]
        public static CalculatorResult JupiterInLibra(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Jupiter, ZodiacName.Libra, time), new[] { PlanetName.Jupiter }, new[] { ZodiacName.Libra }, time);
        [HoroscopeCalculator(HoroscopeName.JupiterInScorpio)]
        public static CalculatorResult JupiterInScorpio(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Jupiter, ZodiacName.Scorpio, time), new[] { PlanetName.Jupiter }, new[] { ZodiacName.Scorpio }, time);
        [HoroscopeCalculator(HoroscopeName.JupiterInSagittarius)]
        public static CalculatorResult JupiterInSagittarius(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Jupiter, ZodiacName.Sagittarius, time), new[] { PlanetName.Jupiter }, new[] { ZodiacName.Sagittarius }, time);
        [HoroscopeCalculator(HoroscopeName.JupiterInCapricorn)]
        public static CalculatorResult JupiterInCapricorn(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Jupiter, ZodiacName.Capricorn, time), new[] { PlanetName.Jupiter }, new[] { ZodiacName.Capricorn }, time);
        [HoroscopeCalculator(HoroscopeName.JupiterInAquarius)]
        public static CalculatorResult JupiterInAquarius(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Jupiter, ZodiacName.Aquarius, time), new[] { PlanetName.Jupiter }, new[] { ZodiacName.Aquarius }, time);
        [HoroscopeCalculator(HoroscopeName.JupiterInPisces)]
        public static CalculatorResult JupiterInPisces(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Jupiter, ZodiacName.Pisces, time), new[] { PlanetName.Jupiter }, new[] { ZodiacName.Pisces }, time);

        //VENUS
        [HoroscopeCalculator(HoroscopeName.VenusInAries)]
        public static CalculatorResult VenusInAries(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Venus, ZodiacName.Aries, time), new[] { PlanetName.Venus }, new[] { ZodiacName.Aries }, time);
        [HoroscopeCalculator(HoroscopeName.VenusInTaurus)]
        public static CalculatorResult VenusInTaurus(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Venus, ZodiacName.Taurus, time), new[] { PlanetName.Venus }, new[] { ZodiacName.Taurus }, time);
        [HoroscopeCalculator(HoroscopeName.VenusInGemini)]
        public static CalculatorResult VenusInGemini(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Venus, ZodiacName.Gemini, time), new[] { PlanetName.Venus }, new[] { ZodiacName.Gemini }, time);
        [HoroscopeCalculator(HoroscopeName.VenusInCancer)]
        public static CalculatorResult VenusInCancer(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Venus, ZodiacName.Cancer, time), new[] { PlanetName.Venus }, new[] { ZodiacName.Cancer }, time);
        [HoroscopeCalculator(HoroscopeName.VenusInLeo)]
        public static CalculatorResult VenusInLeo(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Venus, ZodiacName.Leo, time), new[] { PlanetName.Venus }, new[] { ZodiacName.Leo }, time);
        [HoroscopeCalculator(HoroscopeName.VenusInVirgo)]
        public static CalculatorResult VenusInVirgo(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Venus, ZodiacName.Virgo, time), new[] { PlanetName.Venus }, new[] { ZodiacName.Virgo }, time);
        [HoroscopeCalculator(HoroscopeName.VenusInLibra)]
        public static CalculatorResult VenusInLibra(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Venus, ZodiacName.Libra, time), new[] { PlanetName.Venus }, new[] { ZodiacName.Libra }, time);
        [HoroscopeCalculator(HoroscopeName.VenusInScorpio)]
        public static CalculatorResult VenusInScorpio(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Venus, ZodiacName.Scorpio, time), new[] { PlanetName.Venus }, new[] { ZodiacName.Scorpio }, time);
        [HoroscopeCalculator(HoroscopeName.VenusInSagittarius)]
        public static CalculatorResult VenusInSagittarius(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Venus, ZodiacName.Sagittarius, time), new[] { PlanetName.Venus }, new[] { ZodiacName.Sagittarius }, time);
        [HoroscopeCalculator(HoroscopeName.VenusInCapricorn)]
        public static CalculatorResult VenusInCapricorn(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Venus, ZodiacName.Capricorn, time), new[] { PlanetName.Venus }, new[] { ZodiacName.Capricorn }, time);
        [HoroscopeCalculator(HoroscopeName.VenusInAquarius)]
        public static CalculatorResult VenusInAquarius(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Venus, ZodiacName.Aquarius, time), new[] { PlanetName.Venus }, new[] { ZodiacName.Aquarius }, time);
        [HoroscopeCalculator(HoroscopeName.VenusInPisces)]
        public static CalculatorResult VenusInPisces(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Venus, ZodiacName.Pisces, time), new[] { PlanetName.Venus }, new[] { ZodiacName.Pisces }, time);


        //SATURN
        [HoroscopeCalculator(HoroscopeName.SaturnInAries)]
        public static CalculatorResult SaturnInAries(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Saturn, ZodiacName.Aries, time), new[] { PlanetName.Saturn }, new[] { ZodiacName.Aries }, time);
        [HoroscopeCalculator(HoroscopeName.SaturnInTaurus)]
        public static CalculatorResult SaturnInTaurus(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Saturn, ZodiacName.Taurus, time), new[] { PlanetName.Saturn }, new[] { ZodiacName.Taurus }, time);
        [HoroscopeCalculator(HoroscopeName.SaturnInGemini)]
        public static CalculatorResult SaturnInGemini(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Saturn, ZodiacName.Gemini, time), new[] { PlanetName.Saturn }, new[] { ZodiacName.Gemini }, time);
        [HoroscopeCalculator(HoroscopeName.SaturnInCancer)]
        public static CalculatorResult SaturnInCancer(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Saturn, ZodiacName.Cancer, time), new[] { PlanetName.Saturn }, new[] { ZodiacName.Cancer }, time);
        [HoroscopeCalculator(HoroscopeName.SaturnInLeo)]
        public static CalculatorResult SaturnInLeo(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Saturn, ZodiacName.Leo, time), new[] { PlanetName.Saturn }, new[] { ZodiacName.Leo }, time);
        [HoroscopeCalculator(HoroscopeName.SaturnInVirgo)]
        public static CalculatorResult SaturnInVirgo(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Saturn, ZodiacName.Virgo, time), new[] { PlanetName.Saturn }, new[] { ZodiacName.Virgo }, time);
        [HoroscopeCalculator(HoroscopeName.SaturnInLibra)]
        public static CalculatorResult SaturnInLibra(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Saturn, ZodiacName.Libra, time), new[] { PlanetName.Saturn }, new[] { ZodiacName.Libra }, time);
        [HoroscopeCalculator(HoroscopeName.SaturnInScorpio)]
        public static CalculatorResult SaturnInScorpio(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Saturn, ZodiacName.Scorpio, time), new[] { PlanetName.Saturn }, new[] { ZodiacName.Scorpio }, time);
        [HoroscopeCalculator(HoroscopeName.SaturnInSagittarius)]
        public static CalculatorResult SaturnInSagittarius(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Saturn, ZodiacName.Sagittarius, time), new[] { PlanetName.Saturn }, new[] { ZodiacName.Sagittarius }, time);
        [HoroscopeCalculator(HoroscopeName.SaturnInCapricorn)]
        public static CalculatorResult SaturnInCapricorn(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Saturn, ZodiacName.Capricorn, time), new[] { PlanetName.Saturn }, new[] { ZodiacName.Capricorn }, time);
        [HoroscopeCalculator(HoroscopeName.SaturnInAquarius)]
        public static CalculatorResult SaturnInAquarius(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Saturn, ZodiacName.Aquarius, time), new[] { PlanetName.Saturn }, new[] { ZodiacName.Aquarius }, time);
        [HoroscopeCalculator(HoroscopeName.SaturnInPisces)]
        public static CalculatorResult SaturnInPisces(Time time) => CalculatorResult.New(Calculate.IsPlanetInSign(PlanetName.Saturn, ZodiacName.Pisces, time), new[] { PlanetName.Saturn }, new[] { ZodiacName.Pisces }, time);



        #endregion

        #region MARRIAGE

        [HoroscopeCalculator(HoroscopeName.MarsVenusIn7th)]
        public static CalculatorResult MarsVenusIn7th(Time time)
        {
            //When Mars and Venus are in the 7th, the boy or girl concerned will have strong sex instincts
            //and such an individual should be mated to one who has similar instincts

            //mars in 7th at birth
            var marsIn7th = Calculate.IsPlanetInHouse(PlanetName.Mars, HouseName.House7, time);

            //venus in 7th at birth
            var venusIn7th = Calculate.IsPlanetInHouse(PlanetName.Venus, HouseName.House7, time);

            //occuring if all conditions met
            var occuring = marsIn7th && venusIn7th;

            return CalculatorResult.New(occuring, new[] { HouseName.House7 }, new[] { PlanetName.Mars, PlanetName.Venus }, time);

        }

        [HoroscopeCalculator(HoroscopeName.MercuryOrJupiterIn7th)]
        public static CalculatorResult MercuryOrJupiterIn7th(Time time)
        {
            // Mercury or Jupiter in the 7th, makes one under-sexed.
            // And such an individual should not be mated to a person with strong sex instincts.

            //Mercury in 7th at birth
            var mercuryIn7th = Calculate.IsPlanetInHouse(PlanetName.Mercury, HouseName.House7, time);

            //Jupiter in 7th at birth
            var jupiterIn7th = Calculate.IsPlanetInHouse(PlanetName.Jupiter, HouseName.House7, time);

            //occuring if either conditions met
            var occuring = mercuryIn7th || jupiterIn7th;

            return new() { Occuring = occuring };
        }

        [HoroscopeCalculator(HoroscopeName.LeoLagna7thLordSaturnIn2)]
        public static CalculatorResult LeoLagna7thLordSaturnIn2(Time time)
        {
            //When Leo is Lagna and the 7th lord Saturn is in the 2nd, the
            // husband will be subservient to the wife carrying out all her orders.

            //lagna is leo
            var leoIsLagna = Calculate.HouseSignName(HouseName.House1, time) == ZodiacName.Leo;

            //is 7th lord saturn
            var isLord7thSaturn = Calculate.LordOfHouse(HouseName.House7, time) ==
                                  PlanetName.Saturn;

            //is saturn in 2nd
            var isSaturnIn2nd =
                Calculate.IsPlanetInHouse(PlanetName.Saturn, HouseName.House2, time);


            //occuring conditions met
            var occuring = leoIsLagna && isLord7thSaturn && isSaturnIn2nd;

            return new() { Occuring = occuring };
        }

        [HoroscopeCalculator(HoroscopeName.SaturnIn7thNotLagnaLord)]
        public static CalculatorResult SaturnIn7thNotLagnaLord(Time time)
        {
            //Saturn in the 7th house is also indicative of unhappiness in marriage
            // unless Saturn happens to be either lord of Lagna or lord of the 7th.

            //is saturn in 7th house
            var isSaturnIn7th = Calculate.IsPlanetInHouse(PlanetName.Saturn, HouseName.House7, time);

            //saturn is not lord of lagna
            var saturnNotLagnaLord =
                Calculate.LordOfHouse(HouseName.House1, time) != PlanetName.Saturn;

            //saturn is not lord of 7th
            var saturnNot7thLord =
                Calculate.LordOfHouse(HouseName.House7, time) != PlanetName.Saturn;


            //occuring conditions met
            var occuring = isSaturnIn7th && saturnNotLagnaLord && saturnNot7thLord;

            return new() { Occuring = occuring };
        }

        [HoroscopeCalculator(HoroscopeName.MarsIn7thNoBenefics)]
        public static CalculatorResult MarsIn7thNoBenefics(Time time)
        {
            //If Kuja is in the 7th house unaspected or not joined by benefics,
            //there will be frequent quarrels in the married life often leading to
            //misunderstandings and separation.

            //is mars in 7th house
            var isMarsIn7th = Calculate.IsPlanetInHouse(PlanetName.Mars, HouseName.House7, time);

            //no benefics aspecting 7th house
            var beneficsNotAspect7th = !Calculate.IsBeneficPlanetAspectHouse(HouseName.House7, time);

            //no benefics located in 7th
            var beneficNotFoundIn7th = !Calculate.IsBeneficPlanetInHouse(HouseName.House7, time);

            //occuring conditions met
            var occuring = isMarsIn7th && beneficsNotAspect7th && beneficNotFoundIn7th;

            return new() { Occuring = occuring };
        }

        [HoroscopeCalculator(HoroscopeName.SunVenusIn5th7th9th)]
        public static CalculatorResult SunVenusIn5th7th9th(Time time)
        {
            //According to Prasna Marga the famous Kerala work on Astrology, if
            //the Sun and Venus occupy the 5th, 7th, or 9th house then the native will
            //lack marital happiness.
            //
            //NOTE : *is intepreted as in the same house at the same time

            //is sun & venus in 5th
            var isSunIn5th = Calculate.IsPlanetInHouse(PlanetName.Sun, HouseName.House5, time);
            var isVenusIn5th = Calculate.IsPlanetInHouse(PlanetName.Venus, HouseName.House5, time);
            var sunAndVenusIn5th = isSunIn5th && isVenusIn5th;

            //is sun & venus in 7th
            var isSunIn7th = Calculate.IsPlanetInHouse(PlanetName.Sun, HouseName.House7, time);
            var isVenusIn7th = Calculate.IsPlanetInHouse(PlanetName.Venus, HouseName.House7, time);
            var sunAndVenusIn7th = isSunIn7th && isVenusIn7th;

            //is sun & venus in 9th
            var isSunIn9th = Calculate.IsPlanetInHouse(PlanetName.Sun, HouseName.House9, time);
            var isVenusIn9th = Calculate.IsPlanetInHouse(PlanetName.Venus, HouseName.House9, time);
            var sunAndVenusIn9th = isSunIn9th && isVenusIn9th;


            //occuring conditions met
            var occuring = sunAndVenusIn5th || sunAndVenusIn7th || sunAndVenusIn9th;

            return new() { Occuring = occuring };
        }

        [HoroscopeCalculator(HoroscopeName.Lord7And1Friends)]
        public static CalculatorResult Lord7And1Friends(Time time)
        {

            //If the lords of the 7th and 1st are friends then the native will be loved
            //by his wife. Otherwise there will be no harmony.


            //get lord of 7th and 1st house
            var lord7 = Calculate.LordOfHouse(HouseName.House7, time);
            var lord1 = Calculate.LordOfHouse(HouseName.House1, time);

            //get the relationship
            var lord7And1Relationship = Calculate.PlanetCombinedRelationshipWithPlanet(lord7, lord1, time);

            //occuring only if best friends or normal friends nothing else
            var occuring = (lord7And1Relationship == PlanetToPlanetRelationship.BestFriend) ||
                           (lord7And1Relationship == PlanetToPlanetRelationship.Friend);

            return new() { Occuring = occuring };
        }

        [HoroscopeCalculator(HoroscopeName.Lord7And1NotFriends)]
        public static CalculatorResult Lord7And1NotFriends(Time time)
        {

            //If the lords of the 7th and 1st are friends then the native will be loved
            //by his wife. Otherwise* there will be no harmony.
            //
            //* Intepreted as enemies or bitter enemies only, neutral is not inlcuded


            //get lord of 7th and 1st house
            var lord7 = Calculate.LordOfHouse(HouseName.House7, time);
            var lord1 = Calculate.LordOfHouse(HouseName.House1, time);

            //get the relationship
            var lord7And1Relationship = Calculate.PlanetCombinedRelationshipWithPlanet(lord7, lord1, time);

            //occuring only if bitter enemies or normal enemies nothing else
            var occuring = (lord7And1Relationship == PlanetToPlanetRelationship.BitterEnemy) ||
                           (lord7And1Relationship == PlanetToPlanetRelationship.Enemy);

            return new() { Occuring = occuring };
        }

        [HoroscopeCalculator(HoroscopeName.SaturnIn7th)]
        public static CalculatorResult SaturnIn7th(Time time)
        {
            //Saturn in the 7th
            //confers stability in the marriage but the, husband or wife manifests
            //coldness and not warmth.

            //is saturn in 7th house
            var isSaturnIn7th = Calculate.IsPlanetInHouse(PlanetName.Saturn, HouseName.House7, time);

            //occuring conditions met
            var occuring = isSaturnIn7th;

            return new() { Occuring = occuring };
        }

        #endregion

        #region GENERAL RULES

        //[HoroscopeCalculator(HoroscopeName.LordInTrine)]
        //public static CalculatorResult LordInTrine(Time time)
        //{
        //    //The lords of trines are always ausp1c10us and produce good


        //    return new()
        //    {
        //        Occuring = AstronomicalCalculator.GetPlanetRasiSign(PlanetName.Sun, person.BirthTime).GetSignName() == ZodiacName.Aries
        //    };
        //}

        #endregion

        //CUSTOM
        [HoroscopeCalculator(HoroscopeName.GeminiRisingWithEvilPlanet)]
        public static CalculatorResult GeminiRisingWithEvilPlanet(Time time)
        {
            //1.gemini rising 
            var geminiRising = Calculate.HouseSignName(HouseName.House1, time) == ZodiacName.Gemini;

            //2.find evil planets in gemini
            //get planets in sign
            var planetsInSign = Calculate.PlanetInSign(ZodiacName.Gemini, time);
            //filer in only evil (malefic) planets 
            var evilPlanets = planetsInSign.Where(planet => Calculate.IsPlanetMalefic(planet, time));
            //mark if evil planets found in sign
            var evilPlanetFound = evilPlanets.Any();


            //both must be true for event to occur
            var occuring = geminiRising && evilPlanetFound;

            //extra info
            return CalculatorResult.New(occuring, new[] { HouseName.House1 }, evilPlanets.ToArray(), new[] { ZodiacName.Gemini }, time);
        }

        [HoroscopeCalculator(HoroscopeName.AriesRisingWithEvilPlanet)]
        public static CalculatorResult AriesRisingWithEvilPlanet(Time time)
        {
            //Mental affliction and derangement are also likely since Saturn and the Moon are in Aries.

            //1.aries rising 
            var ariesRising = Calculate.HouseSignName(HouseName.House1, time) == ZodiacName.Aries;

            //2.find if Saturn and the Moon are in Aries.
            //get planets in sign
            var planetsInSign = Calculate.PlanetInSign(ZodiacName.Aries, time);
            var evilPlanetFound = planetsInSign.Contains(PlanetName.Saturn) || planetsInSign.Contains(PlanetName.Moon);


            //both must be true for event to occur
            var occuring = ariesRising && evilPlanetFound;

            //extra info
            return CalculatorResult.New(occuring, new[] { HouseName.House1 }, new[] { PlanetName.Saturn, PlanetName.Moon }, new[] { ZodiacName.Aries }, time);
        }

    }
}