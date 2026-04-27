import React, { useState, useEffect } from 'react';
import Tree from 'rc-tree';
import 'rc-tree/assets/index.css';
import './ChangePathsTree.css';
import svgIconDown from '../../../../asset/svgIconDown.svg';
import svgIconRight from '../../../../asset/svgIconRight.svg';

export default function FolderTree(props) {
    const { treeData, defaultPath, pathChange } = props;
    const [expandedKeys, setExpandedKeys] = useState(defaultPath);
    const [selectedKeys, setSelectedKeys] = useState([defaultPath[defaultPath.length - 1]]);

    const onExpand = keys => setExpandedKeys(keys);

    const FolderIconOpen = () => <span class="rc-tree-iconEle rc-tree-icon__open"></span>;
    const FolderIconClosed = () => <span class="rc-tree-iconEle rc-tree-icon__close"></span>;

    // Helper to find the full path for the selected key
    const findPath = (nodes, key, path = []) => {
        for (const node of nodes) {
            const currentPath = [...path, node.key];
            if (node.key === key) return currentPath;
            if (node.children) {
                const result = findPath(node.children, key, currentPath);
                if (result) return result;
            }
        }
        return null;
    };

    const onSelect = keys => {
        setSelectedKeys(keys);
        if (keys.length > 0) {
            const fullPath = findPath(treeData, keys[0]);
            pathChange(fullPath); // Pass the full path array
        } else {
            pathChange([]); // No selection
        }
    };

    // Custom switcher icon for expand/collapse arrows
    const switcherIcon = ({ expanded, isLeaf }) => {
        if (isLeaf) return null;
        return (
            <span style={{ marginRight: 4 }}>
                {expanded ? <img src={svgIconDown} /> : <img src={svgIconRight} />}
            </span>
        );
    };

    return (
        <Tree
            treeData={treeData}
            expandedKeys={expandedKeys}
            selectedKeys={selectedKeys}
            onExpand={onExpand}
            onSelect={onSelect}
            switcherIcon={switcherIcon}
            showIcon
            icon={({ expanded, isLeaf }) =>
                isLeaf ? null : expanded ? <FolderIconOpen /> : <FolderIconClosed />
            }
        />
    );
}