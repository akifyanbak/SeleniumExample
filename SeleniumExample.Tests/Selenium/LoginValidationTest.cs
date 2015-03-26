using System;
using System.Configuration;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using Assert = NUnit.Framework.Assert;

namespace SeleniumExample.Tests.Selenium
{
    [TestFixture]
    public class LoginValidationTest
    {
        private IWebDriver _driverFirefox;
        private IWebDriver _driverCrhome;
        private IWebDriver _driverInternetExplorer;
        private StringBuilder _verificationErrors;

        /// <summary>
        /// App.config dosyasından "Development" değerini çekiyor
        /// </summary>
        public string BaseUrl = ConfigurationManager.AppSettings["Development"] + "/Account/Login";

        /// <summary>
        /// Test çalıştığında ilk olarak buraya girer. Constructor veya [TestInitialize()] attribute'ü gibi düşünülebilir
        /// </summary>
        [SetUp]
        public void SetupTest()
        {
            _driverFirefox = new FirefoxDriver();
            _driverCrhome = new ChromeDriver("C:\\WORKSPACE\\Selenium");
            _driverInternetExplorer = new InternetExplorerDriver("C:\\WORKSPACE\\Selenium");
            _verificationErrors = new StringBuilder();
        }

        /// <summary>
        /// Test bittikten sonra buraya giriyor, tarayıcıları kapatıyor. Assert.AreEqual ile test sonucunu veriyor.
        /// </summary>
        [TearDown]
        public void TeardownTest()
        {
            try
            {
                _driverFirefox.Quit();
                _driverCrhome.Quit();
                _driverInternetExplorer.Quit();
            }
            catch (Exception)
            {
                // ignored
            }
            Assert.AreEqual("", _verificationErrors.ToString());

        }

        /// <summary>
        /// Test metotu
        /// </summary>
        [Test]
        public void TheCreateFTest()
        {
            GenericBrowser(_driverFirefox);
            GenericBrowser(_driverCrhome);
            GenericBrowser(_driverInternetExplorer);
        }

        /// <summary>
        /// IWebDriver interface'ine sahip değer alan fonksiyonu
        /// </summary>
        /// <param name="webDriver"></param>
        public void GenericBrowser(IWebDriver webDriver)
        {
            // App.config'den aldığımız bağlantıyı tarayıcıda başlatıyoruz.
            webDriver.Navigate().GoToUrl(BaseUrl);

            // form-control class'ı olan element'leri yakalıyoruz.
            var formControls = webDriver.FindElements(By.CssSelector(".form-control"));

            //Yakaladığımız form control'lerini tek tek döngüye sokuyoruz
            foreach (var formControl in formControls)
            {
                // Form control'ünün data-val attribute'ü var ve "true" ise ve control tipi "select" (dropdownlist) değil ise döngüye alıyoruz.
                if (formControl.GetAttribute("data-val") != null && formControl.GetAttribute("data-val").ToLower() == "true" && formControl.TagName.ToLowerInvariant() != "select")
                {
                    // Olası bütün validasyon attribute'lerini alıyoruz.
                    string inputId = formControl.GetAttribute("id");
                    string requriedDatetime = formControl.GetAttribute("data-val-date");
                    string lengtMax = formControl.GetAttribute("data-val-length-max");
                    string lengtMin = formControl.GetAttribute("data-val-length-min");
                    string requriedVal = formControl.GetAttribute("data-val-required");
                    string requriedNumber = formControl.GetAttribute("data-val-number");

                    // Validasyon kontrollerine aykırı değerler girip, mutlaka hata vermesini sağlıyoruz.
                    if (lengtMax != null)
                    {
                        formControl.SendKeys(Keys.Backspace);
                        formControl.SendKeys(new string(Enumerable.Repeat('a', Convert.ToInt32(lengtMin) + 1).ToArray()));
                        webDriver.FindElement(By.CssSelector("input.btn.btn-default")).Click();
                        IsWarningGeneric(inputId, webDriver);
                    }
                    if (lengtMin != null || requriedVal != null)
                    {
                        formControl.SendKeys(Keys.Backspace);
                        formControl.SendKeys("");
                        webDriver.FindElement(By.CssSelector("input.btn.btn-default")).Click();
                        IsWarningGeneric(inputId, webDriver);
                    }
                    if (requriedNumber != null || requriedDatetime != null)
                    {
                        formControl.SendKeys(Keys.Backspace);
                        formControl.SendKeys("a");
                        webDriver.FindElement(By.CssSelector("input.btn.btn-default")).Click();
                        IsWarningGeneric(inputId, webDriver);
                    }
                }
            }
        }

        //data-valmsg-for dan hangisine aitse ordan mesaj var mı diye bakıyoruz. Varsa işlemimiz doğrudur.
        private void IsWarningGeneric(string inputId, IWebDriver webDriver)
        {
            if (webDriver.FindElements(By.CssSelector("span[data-valmsg-for='" + inputId + "']")).Count < 1)
                _verificationErrors.Append("-" + inputId + " id'li input hata mesajı vermiyor -");
        }
    }
}
