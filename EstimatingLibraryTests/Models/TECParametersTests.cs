using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestLibrary.ModelTestingUtilities;

namespace Models
{
    [TestClass()]
    public class TECParametersTests
    {
        [TestMethod()]
        public void UpdateConstantsTest()
        {
            TECParameters parameters = new TECParameters();
            TECParameters newLabor = ModelCreation.TestParameters(new Random(0));
            parameters.UpdateConstants(newLabor);

            Assert.AreEqual(newLabor.PMCoef, parameters.PMCoef);
            Assert.AreEqual(newLabor.PMCoefStdError, parameters.PMCoefStdError);
            Assert.AreEqual(newLabor.PMExtenedCoef, parameters.PMExtenedCoef);
            Assert.AreEqual(newLabor.PMRate, parameters.PMRate);

            Assert.AreEqual(newLabor.ENGCoef, parameters.ENGCoef);
            Assert.AreEqual(newLabor.ENGCoefStdError, parameters.ENGCoefStdError);
            Assert.AreEqual(newLabor.ENGExtenedCoef, parameters.ENGExtenedCoef);
            Assert.AreEqual(newLabor.ENGRate, parameters.ENGRate);

            Assert.AreEqual(newLabor.SoftCoef, parameters.SoftCoef);
            Assert.AreEqual(newLabor.SoftCoefStdError, parameters.SoftCoefStdError);
            Assert.AreEqual(newLabor.SoftExtenedCoef, parameters.SoftExtenedCoef);
            Assert.AreEqual(newLabor.SoftRate, parameters.SoftRate);

            Assert.AreEqual(newLabor.GraphCoef, parameters.GraphCoef);
            Assert.AreEqual(newLabor.GraphCoefStdError, parameters.GraphCoefStdError);
            Assert.AreEqual(newLabor.GraphExtenedCoef, parameters.GraphExtenedCoef);
            Assert.AreEqual(newLabor.GraphRate, parameters.GraphRate);

            Assert.AreEqual(newLabor.ElectricalRate, parameters.ElectricalRate);
            Assert.AreEqual(newLabor.ElectricalNonUnionRate, parameters.ElectricalNonUnionRate);
            Assert.AreEqual(newLabor.ElectricalSuperRate, parameters.ElectricalSuperRate);
            Assert.AreEqual(newLabor.ElectricalSuperNonUnionRate, parameters.ElectricalSuperNonUnionRate);
            Assert.AreEqual(newLabor.ElectricalSuperRatio, parameters.ElectricalSuperRatio);

        }
    }
}