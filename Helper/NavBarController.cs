using System;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class NavBarController : MonoBehaviour
    {
        [Serializable] public class NavBarTreeNode
        {
            [HideInInspector] public string name;
            public NavBar navBar;
            [SerializeField] int currentIndex;
            int currentIndexCheck;
            public List<NavBarTreeNode> treeNodes;
            public void Validate(Action onChange)
            {
                if (navBar == null) return;
                if (currentIndexCheck == currentIndex) return;
                if (currentIndex < 0) currentIndex = 0;
                if (currentIndex >= navBar.items.Count) currentIndex = 0;

                currentIndexCheck = currentIndex;

                navBar.ForceUpdate(currentIndex);

                onChange?.Invoke();
            }
        }

        [SerializeField] bool setup;
        [SerializeField] bool updateNavBars;
        [SerializeField] List<NavBarTreeNode> navBars;

        private void OnValidate()
        {
            HandleSetUp();
            HandleUpdateNavBars();
        }

        void HandleSetUp()
        {
            if (!setup) return;
            setup = false;

            foreach(var node in navBars)
            {
                FindActiveChildren(node);
            }
        }


        void FindActiveChildren(NavBarTreeNode currentNode)
        {
            var navBar = currentNode.navBar;
            if (navBar == null) return;
            currentNode.name = navBar.ToString();
            currentNode.treeNodes = new();

            var children = new Queue<Transform>();
            var targetTransform = navBar.items[navBar.ActiveIndex()].transform;
            children.Enqueue(targetTransform);

            while(children.Count > 0)
            {
                var currentTransform = children.Dequeue();
                var currentNavBar = currentTransform.GetComponent<NavBar>();
                if (currentNavBar != null)
                {
                    var newNode = new NavBarTreeNode() {
                        navBar = currentNavBar,
                        name = targetTransform.ToString(),
                    };
                    currentNode.treeNodes.Add(newNode);
                    FindActiveChildren(newNode);
                    continue;
                }

                foreach(Transform child in currentTransform)
                {
                    children.Enqueue(child);
                }

            }
        }

        void HandleUpdateNavBars()
        {
            foreach(var node in navBars)
            {
                Recur(node);
            }

            void Recur(NavBarTreeNode currentNode)
            {
                if (currentNode.treeNodes == null) return;
                currentNode.Validate(() => {
                    FindActiveChildren(currentNode);
                });
                foreach(var node in currentNode.treeNodes)
                {
                    Recur(node);
                }
            }
        }
    }
}
