using Mma.Common.models;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Test.Mma.Common.TestCaseSources {
    internal class WindDataTestSource {
        public static IEnumerable CalmWindDataTestCases {
            get {
                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = null,
                        MaximumWindDirection = null,
                        AverageWindDirection = null,
                        MaximumWindSpeed = null,
                        MinimumWindDirection = null
                    },
                    false); 

                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = 0,
                        MaximumWindDirection = 0,
                        AverageWindDirection = 0,
                        MaximumWindSpeed = 0,
                        MinimumWindDirection = 0
                    },
                    true); 
                
            }
        }

        public static IEnumerable WindDataFullTestCases {
            get {
                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = 8,
                        MaximumWindDirection = null,
                        AverageWindDirection = 20,
                        MaximumWindSpeed = null,
                        MinimumWindDirection = null
                    },
                    "02008KT");

                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = 0,
                        MaximumWindDirection = 0,
                        AverageWindDirection = 0,
                        MaximumWindSpeed = 0,
                        MinimumWindDirection = 0
                    },
                    "00000KT");

                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = 2,
                        MaximumWindDirection = 180,
                        AverageWindDirection = 50,
                        MaximumWindSpeed = 0,
                        MinimumWindDirection = 100
                    },
                    "VRB02KT");

                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = 22,
                        MaximumWindDirection = null,
                        AverageWindDirection = 330,
                        MaximumWindSpeed = 34,
                        MinimumWindDirection = 100
                    },
                    "33022G34KT");

                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = 16,
                        MaximumWindDirection = 190,
                        AverageWindDirection = 160,
                        MaximumWindSpeed = null,
                        MinimumWindDirection = 120
                    },
                    "16016KT 120V190");

                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = 15,
                        MaximumWindDirection = 270,
                        AverageWindDirection = 210,
                        MaximumWindSpeed = 28,
                        MinimumWindDirection = 180
                    },
                    "21015G28KT 180V270");

                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = 70,
                        MaximumWindDirection = null,
                        AverageWindDirection = 270,
                        MaximumWindSpeed = 100,
                        MinimumWindDirection = null
                    },
                    "27070GP99KT");

                yield return new TestCaseData(
                    new WindData {
                        AverageWindSpeed = 12,
                        MaximumWindDirection = null,
                        AverageWindDirection = null,
                        MaximumWindSpeed = null,
                        MinimumWindDirection = null
                    },
                    "///12KT");
            }
        }

        public static IEnumerable VariationInWindDirectionInRangeAndLessThan3TestCases {
            get {
                yield return new TestCaseData("005", "005", "05", "");
                yield return new TestCaseData(null, null, null, "");
                yield return new TestCaseData("-005", "065", "03", "");
                yield return new TestCaseData("005", "065", "00", "");
                yield return new TestCaseData("005", "065", "-01", "");
                yield return new TestCaseData("000", "059", "03", "");
                yield return new TestCaseData("000", "070", "03", "");
                yield return new TestCaseData("000", "180", "04", "");
                yield return new TestCaseData("000", "179", "P99", " 000V179");
                yield return new TestCaseData("000", "181", "05", "");
                yield return new TestCaseData("///", "181", "05", "");
                yield return new TestCaseData("000", "170", "04", " 000V170");
            }
        } 
       
        public static IEnumerable SurfaceWindSpeedGreaterThan100TestCases {
            get {
                yield return new TestCaseData("05", "05");
                yield return new TestCaseData("01", "01");
                yield return new TestCaseData("99", "99");
                yield return new TestCaseData("-05", "//");
                yield return new TestCaseData("00", "00");
                yield return new TestCaseData("100", "P99");
                yield return new TestCaseData("101", "P99");
                yield return new TestCaseData("//", "//");
                yield return new TestCaseData(null, "//");
            }
        } 
        
        public static IEnumerable SpeedRoundingTestCases {
            get {
                yield return new TestCaseData(null, null);
                yield return new TestCaseData(0.0, 0);
                yield return new TestCaseData(0.4, 0);
                yield return new TestCaseData(0.5, 1);
                yield return new TestCaseData(0.9, 1);
                yield return new TestCaseData(1.4, 1);
            }
        }    
        
        public static IEnumerable DirectionForSpeedLessThan3KnotsTestCases {
            get {
                yield return new TestCaseData("005", "005", "05", "");
                yield return new TestCaseData(null, null, null, "");
                yield return new TestCaseData("-005", "065", "03", "");
                yield return new TestCaseData("005", "065", "00", "VRB");
                yield return new TestCaseData("005", "065", "-01", "");
                yield return new TestCaseData("000", "059", "03", "");
                yield return new TestCaseData("000", "070", "03", "VRB");
                yield return new TestCaseData("000", "180", "02", "");
                yield return new TestCaseData("000", "179", "01", "VRB");
                yield return new TestCaseData("000", "181", "05", "");
                yield return new TestCaseData("000", "170", "P99", "");
                yield return new TestCaseData("///", "181", "05", "");

            }
        }

    }
}
