using System;
using System.Linq;
using System.Collections.Generic;
using CoreNodeModels.Input;
using DesignScript.Builtin;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Models;
using Dynamo.Tests;
using CoreNodeModels;
using CoreNodeModelsWpf;
using NUnit.Framework;
using ProtoCore.Namespace;
using TestUINodes;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class NodeExecutionUITest : DynamoViewModelUnitTest
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");

            base.GetLibrariesToPreload(libraries);
        }

        //case 1 : Node in Freeze and Not execute state. True for all parent nodes.
        [Test]
        [Category("DynamoUI")]
        public void Node_InFreeze_NotExecuteState_Test()
        {
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Freeze the node
            addNode.IsFrozen = true;

            var addNodeVm = ViewModel.CurrentSpaceViewModel.Nodes.First(x => x.NodeLogic == addNode);
            Assert.IsNotNull(addNodeVm);
            
            //Check the Freeze property. Assuming only one node is selected
            //this property is fetched from Nodeviewmodel. Context Menu on Workspace,
            //Context menu on Node and Edit Menu toolbar refers to the same location.
            Assert.AreEqual(addNodeVm.IsFrozenExplicitly, true);
            Assert.AreEqual(addNodeVm.CanToggleFrozen, true);
        }

        //case 2 : Node in Freeze and execute state. True for all child nodes.
        [Test]
        [Category("DynamoUI")]
        public void Node_InFreeze_ExecuteState_Test()
        {
            var model = ViewModel.Model;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));

            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode1, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode2, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(addNode, 0, 0, true, false));

            //connect them up
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode1.GUID, 0, PortType.Output,
                DynamoModel.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.End));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode2.GUID, 0, PortType.Output,
                DynamoModel.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 1, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.End));

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 3);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

            //Now Freeze the NumberNode1.
            numberNode1.IsFrozen = true;
           
            //Get the ViewModel of the number node and check the Freeze property.
            var numberNodevm = ViewModel.CurrentSpaceViewModel.Nodes.First(x => x.NodeLogic == numberNode1);
            Assert.IsNotNull(numberNodevm);
            Assert.AreEqual(numberNodevm.IsFrozenExplicitly, true);
            Assert.AreEqual(numberNodevm.CanToggleFrozen, true);

            //Get the ViewModel of add node and check the Freeze property. This node is a child node of numbernode1.
            //so this node should be in Frozen and Executing state.
            var addNodeVm = ViewModel.CurrentSpaceViewModel.Nodes.First(x => x.NodeLogic == addNode);
            Assert.IsNotNull(addNodeVm);
            Assert.AreEqual(addNodeVm.IsFrozenExplicitly, false);
            Assert.AreEqual(addNodeVm.CanToggleFrozen, false);
        }

        //case 3 : Node in Freeze and temporary state. 
        [Test]
        [Category("DynamoUI")]
        public void Node_InTemporaryFreeze_State()
        {
            var model = ViewModel.Model;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));

            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode1, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode2, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(addNode, 0, 0, true, false));

            //connect them up
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode1.GUID, 0, PortType.Output,
                DynamoModel.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.End));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode2.GUID, 0, PortType.Output,
                DynamoModel.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 1, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.End));

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 3);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

            //Now Freeze the add node. This node has two input nodes. Note that
            //input nodes are not frozen.
            addNode.IsFrozen = true;
           
            //Get the ViewModel of add node and check the Freeze property.
            //This node should be in Frozen and not Executing state.
            var addNodeVm = ViewModel.CurrentSpaceViewModel.Nodes.First(x => x.NodeLogic == addNode);
            Assert.IsNotNull(addNodeVm);
            Assert.AreEqual(addNodeVm.IsFrozenExplicitly, true);
            Assert.AreEqual(addNodeVm.CanToggleFrozen, true);

            //Now freeze NumberNode1.
            numberNode1.IsFrozen = true;
           
            //Get the ViewModel of add node and check the Freeze property.
            //This node should be in Frozen and not Executing state.
            var numberNode1Vm = ViewModel.CurrentSpaceViewModel.Nodes.First(x => x.NodeLogic == numberNode1);
            Assert.IsNotNull(numberNode1Vm);
            Assert.AreEqual(numberNode1Vm.IsFrozenExplicitly, true);
            Assert.AreEqual(numberNode1Vm.CanToggleFrozen, true);

            //Now check the add node. Freeze property will be unchecked and disabled.
            Assert.AreEqual(addNodeVm.IsFrozenExplicitly, true);
            Assert.AreEqual(addNodeVm.CanToggleFrozen, false);
        }

        [Test]
        [Category("DynamoUI")]
        public void Undo_Freeze_Test()
        {
            var model = ViewModel.Model;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));

            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode1, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode2, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(addNode, 0, 0, true, false));

            //connect them up
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode1.GUID, 0, PortType.Output,
                DynamoModel.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.End));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode2.GUID, 0, PortType.Output,
                DynamoModel.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 1, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.End));

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 3);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

            var addNodeVm = ViewModel.CurrentSpaceViewModel.Nodes.First(x => x.NodeLogic == addNode);
            Assert.IsNotNull(addNodeVm);

            var numberNode1Vm = ViewModel.CurrentSpaceViewModel.Nodes.First(x => x.NodeLogic == numberNode1);
            Assert.IsNotNull(numberNode1Vm);

            var numberNode2Vm = ViewModel.CurrentSpaceViewModel.Nodes.First(x => x.NodeLogic == numberNode2);
            Assert.IsNotNull(numberNode2Vm);

            //freeze number node1.            
            numberNode1Vm.ToggleIsFrozenCommand.Execute(null);

            Assert.AreEqual(numberNode1Vm.IsFrozenExplicitly, true);
            Assert.AreEqual(numberNode1Vm.CanToggleFrozen, true);

            //add node is in frozen executing state
            Assert.AreEqual(addNodeVm.IsFrozenExplicitly, false);
            Assert.AreEqual(addNodeVm.CanToggleFrozen, false);

            //freeze number node2
            numberNode2Vm.ToggleIsFrozenCommand.Execute(null);

            Assert.AreEqual(numberNode2Vm.IsFrozenExplicitly, true);
            Assert.AreEqual(numberNode2Vm.CanToggleFrozen, true);

            //add node is in frozen executing state
            Assert.AreEqual(addNodeVm.IsFrozenExplicitly, false);
            Assert.AreEqual(addNodeVm.CanToggleFrozen, false);

            ViewModel.CurrentSpace.Undo();

            //numbernode2 unfreeze
            Assert.AreEqual(numberNode2Vm.IsFrozenExplicitly, false);
            Assert.AreEqual(numberNode2Vm.CanToggleFrozen, true);

            //add node is in frozen executing state
            Assert.AreEqual(addNodeVm.IsFrozenExplicitly, false);
            Assert.AreEqual(addNodeVm.CanToggleFrozen, false);

            ViewModel.CurrentSpace.Undo();

            //numbernode1 unfreeze
            Assert.AreEqual(numberNode1Vm.IsFrozenExplicitly, false);
            Assert.AreEqual(numberNode1Vm.CanToggleFrozen, true);

            //add node is in normal state
            Assert.AreEqual(addNodeVm.IsFrozenExplicitly, false);
            Assert.AreEqual(addNodeVm.CanToggleFrozen, true);

        }

        [Test]
        [Category("DynamoUI")]
        public void NewWorkspaceFunctionDefinitionTest()
        {
            // Create code block node and define DS function "test"
            var model = GetModel(); 

            var code = "def test(x:int = 1){return = x;}test();";
            var cbn = new CodeBlockNodeModel(code, 0, 0, model.LibraryServices, new ElementResolver());

            var command = new DynamoModel.CreateNodeCommand(cbn, 0, 0, true, false);
            model.ExecuteCommand(command);

            AssertPreviewValue(cbn.GUID.ToString(), 1);

            // Create empty new workspace 
            ViewModel.NewHomeWorkspaceCommand.Execute(null);

            // Create code block node and invoke test function
            code = "test();";
            cbn = new CodeBlockNodeModel(code, 0, 0, model.LibraryServices, new ElementResolver());
            
            command = new DynamoModel.CreateNodeCommand(cbn, 0, 0, true, false);
            model.ExecuteCommand(command);

            // Assert that function "test" is not defined any longer
            // by asserting null for code block node invoking it.
            AssertPreviewValue(cbn.GUID.ToString(), null);
        }

        [Test]
        public void TestCustomSelectionNodeUpdate()
        {
            var model = GetModel();
            var cdn = new CustomSelection();

            var command = new DynamoModel.CreateNodeCommand(cdn, 0, 0, true, false);
            model.ExecuteCommand(command);

            AssertPreviewValue(cdn.GUID.ToString(), 1);

            cdn.SelectedIndex = -1;
            var vm = new CustomSelectionViewModel(cdn);
            var item = cdn.Items[0];
            vm.RemoveCommand.Execute(item);

            AssertPreviewValue(cdn.GUID.ToString(), null);
            
            cdn.SelectedIndex = 0;
            cdn.OnNodeModified();

            AssertPreviewValue(cdn.GUID.ToString(), 2);
        }

        [Test]
        public void TestSelectionNodeUpdate()
        {
            var model = GetModel();
            var tsn = new TestSelectionNode2();

            tsn.UpdateSelection(new List<int> { 1, 2, 3 });
            var command = new DynamoModel.CreateNodeCommand(tsn, 0, 0, true, false);
            model.ExecuteCommand(command);

            AssertPreviewValue(tsn.GUID.ToString(), 3);

            tsn.ClearSelections();

            AssertPreviewValue(tsn.GUID.ToString(), 0);
        }
        [Test]
        public void TestSelectionNodeUpdate2()
        {
            var model = GetModel();
            var tsn = new TestSelectionNode2();

            tsn.UpdateSelection(new List<int> { 1, 2, 3 });
            var command = new DynamoModel.CreateNodeCommand(tsn, 0, 0, true, false);
            model.ExecuteCommand(command);
            AssertPreviewValue(tsn.GUID.ToString(), 3);

            tsn.ClearSelections();
            AssertPreviewValue(tsn.GUID.ToString(), 0);

            tsn.UpdateSelection(new List<int> { 2, 4, 6,8 });
            AssertPreviewValue(tsn.GUID.ToString(), 4);

            tsn.ClearSelections();
            AssertPreviewValue(tsn.GUID.ToString(), 0);

        }
    }
}
