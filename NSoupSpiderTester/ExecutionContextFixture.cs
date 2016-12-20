using NSoupSpider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace NSoupSpiderTester
{
    [TestClass]
    public class ExecutionContextFixture
    {
        [TestMethod]
        public void SetAndGetContexts1()
        {
            string name = Guid.NewGuid().ToString();
            string value1 = Guid.NewGuid().ToString();
            string value2 = Guid.NewGuid().ToString();

            //1. Outside of ExecutionContextScope: ExecutionContext.Current = null
            Assert.IsNull(ExecutionContext.Current);

            //2. Current ExecutionContext is avilable in the ExecutionContextScope.
            using (ExecutionContextScope contextScope = new ExecutionContextScope())
            {
                ExecutionContext.Current.SetValue(name, value1);
                Assert.AreEqual<string>(value1, ExecutionContext.Current.GetValue<string>(name));
            }

            //3. Nested ExecutionContextScope: ExecutionContextOption.Required
            using (ExecutionContextScope contextScope1 = new ExecutionContextScope())
            {
                ExecutionContext.Current.SetValue(name, value1);
                using (ExecutionContextScope contextScope2 = new ExecutionContextScope(ExecutionContextOption.Required))
                {
                    Assert.AreEqual<string>(value1, ExecutionContext.Current.GetValue<string>(name));

                    ExecutionContext.Current.SetValue(name, value2);
                    Assert.AreEqual<string>(value2, ExecutionContext.Current.GetValue<string>(name));
                }
                Assert.AreEqual<string>(value2, ExecutionContext.Current.GetValue<string>(name));
            }


            //4. Nested ExecutionContextScope: ExecutionContextOption.RequiresNew
            using (ExecutionContextScope contextScope1 = new ExecutionContextScope())
            {
                ExecutionContext.Current.SetValue(name, value1);
                using (ExecutionContextScope contextScope2 = new ExecutionContextScope(ExecutionContextOption.RequiresNew))
                {
                    Assert.IsNotNull(ExecutionContext.Current);
                    Assert.IsNull(ExecutionContext.Current.GetValue<string>(name));
                    ExecutionContext.Current.SetValue(name, value2);
                    Assert.AreEqual<string>(value2, ExecutionContext.Current.GetValue<string>(name));
                }
                Assert.AreEqual<string>(value1, ExecutionContext.Current.GetValue<string>(name));
            }

            //5. Nested ExecutionContextScope: ExecutionContextOption.Supress
            using (ExecutionContextScope contextScope1 = new ExecutionContextScope())
            {
                ExecutionContext.Current.SetValue(name, value1);
                using (ExecutionContextScope contextScope2 = new ExecutionContextScope(ExecutionContextOption.Suppress))
                {
                    Assert.IsNull(ExecutionContext.Current);
                }
                Assert.AreEqual<string>(value1, ExecutionContext.Current.GetValue<string>(name));
            }
        }

        [TestMethod]
        public void SetAndGetContexts2()
        {
            string name = Guid.NewGuid().ToString();
            string value1 = Guid.NewGuid().ToString();
            string value2 = Guid.NewGuid().ToString();

            //1. Change current ExecutionContext will never affect the DependentContext.
            using (ExecutionContextScope contextScope1 = new ExecutionContextScope())
            {
                ExecutionContext.Current.SetValue(name, value1);
                DependentContext depedencyContext = ExecutionContext.Current.DepedentClone();
                ExecutionContext.Current.SetValue(name, value2);

                Task<string> task = Task.Factory.StartNew<string>(() =>
                {
                    using (ExecutionContextScope contextScope2 = new ExecutionContextScope(depedencyContext))
                    {
                        return ExecutionContext.Current.GetValue<string>(name);
                    }
                });

                Assert.AreEqual<string>(value1, task.Result);
                Assert.AreEqual<string>(value2, ExecutionContext.Current.GetValue<string>(name));
            }

            //2. Change DependentContext will never affect the current ExecutionContext.
            using (ExecutionContextScope contextScope1 = new ExecutionContextScope())
            {
                ExecutionContext.Current.SetValue(name, value1);
                DependentContext depedencyContext = ExecutionContext.Current.DepedentClone();
                Task<string> task = Task.Factory.StartNew<string>(() =>
                {
                    using (ExecutionContextScope contextScope2 = new ExecutionContextScope(depedencyContext))
                    {
                        ExecutionContext.Current.SetValue(name, value2);
                        return ExecutionContext.Current.GetValue<string>(name);
                    }
                });

                Assert.AreEqual<string>(value2, task.Result);
                Assert.AreEqual<string>(value1, ExecutionContext.Current.GetValue<string>(name));
            }
        }
    }
}
