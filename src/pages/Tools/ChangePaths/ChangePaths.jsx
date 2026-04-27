import React from 'react';
import { useHistory } from 'react-router-dom'
import style from './changePaths.module.css';
import ChangePathsTree from './ChangePathsTree.jsx';
import CustomButton from '../../../components/CustomButton/CustomButton.jsx';
import axios from 'axios';
import { ApiHelper } from '../../../dvt_api/ApiHelper';
import { useSelector } from 'react-redux';
import { DvtApiPaths } from '../../../dvt_api/DvtApiPaths';

const CustomDialog = React.lazy(() => import('../../../components/CustomDialog/CustomDialog.jsx'));
const FullPageLoader = React.lazy(() => import('../../../components/FullPageLoader/index.jsx'));


function emailPathToKeyPathList(pathStr) {
    if (!pathStr) return [];
    const parts = pathStr.split('/');
    const result = [];
    for (let i = 0; i < parts.length; i++) {
        result.push(parts.slice(0, i + 1).join('/'));
    }
    return result;
}

const ChangePaths = () => {
    const [loader, setLoader] = React.useState(false);
    const [open, setOpen] = React.useState(false);
    const [treeData, setTreeData] = React.useState([]);
    const [defaultLoadPath, setDefaultLoadPath] = React.useState();
    const [defaultLogPath, setDefaultLogPath] = React.useState();
    const [defaultProdPath, setDefaultProdPath] = React.useState();
    const [selectedPath, setSelectedPath] = React.useState([]);
    const [type, setType] = React.useState('loadFolder');
    const userEmail = useSelector(state => state.userInfo.email);
    const [userId, setUserId] = React.useState('');
    let history = useHistory();

    React.useEffect(() => {
        if (userEmail) {
            getFolderList();
            getUserDefaultPaths();
        }
    }, [userEmail]);

    const convertFoldersToTreeData = (folders, parentKey = userEmail) => {
        return folders.map((folder, idx) => {
            const key = parentKey === userEmail ? `${parentKey}/${folder.name}` : `${parentKey}/${folder.name}`;
            return {
                key,
                title: folder.name,
                children: folder.children && folder.children.length > 0
                    ? convertFoldersToTreeData(folder.children, key)
                    : [],
            };
        });
    }

    const getFolderList = async () => {
        try {
            setLoader(true);
            const apiUrl = ApiHelper.getApiUrlWithId(DvtApiPaths.ChangePath.GetFolderList, userEmail);
            const response = await axios.get(apiUrl);
            const apidata = response.data;
            //const apidata = { "Folders": [{ "Name": "T 2", "Children": [{ "Name": "T 2 1", "Children": [] }] }, { "Name": "Test 1", "Children": [{ "Name": "LoadFolder", "Children": [{ "Name": "L3", "Children": [] }] }, { "Name": "Log Folder", "Children": [] }] }, { "Name": "Test 2", "Children": [{ "Name": "Test 2 1", "Children": [{ "Name": "Test 2 1 Load", "Children": [{ "Name": "Test 2 level 4", "Children": [] }] }] }] }] }
            const treeDataList = [{
                key: userEmail,
                title: userEmail,
                disabled: true,
                children: convertFoldersToTreeData(apidata.folders, userEmail),
            }];
            setLoader(false);
            setTreeData(treeDataList);
        } catch (error) {
            setLoader(false);
            console.error('Error fetching folder list:', error);
        }
    };

    const getUserDefaultPaths = async () => {
        try {
            const apiUrl = ApiHelper.getApiUrlWithId(DvtApiPaths.ChangePath.GetUserDefaultPaths, userEmail);
            const response = await axios.get(apiUrl);
            const apidata = response.data;
            const { userInfoId, loadFolder, logFolder, productionFolder } = apidata;
            setUserId(userInfoId);
            setDefaultLoadPath(emailPathToKeyPathList(loadFolder));
            setDefaultLogPath(emailPathToKeyPathList(logFolder));
            setDefaultProdPath(emailPathToKeyPathList(productionFolder));
        } catch (error) {
            console.error('Error fetching user default paths:', error);
        }
    }

    const pathChange = (newPath) => {
        // Handle the logic to update the paths based on user selection
        console.log('New Path Selected:', newPath);
        setSelectedPath(newPath);
    }

    const transformPath = (path) => {
        // Transform the path to a string format for display
        if (!path || !Array.isArray(path)) return '';
        let current = treeData;
        const titles = [];
        for (const key of path) {
            const node = current.find(item => item.key === key);
            if (!node) break;
            titles.push(node.title);
            current = node.children || [];
        }
        return titles.join(' / ');
    }

    const handlePathChange = () => {
        const apiUrl = getApiUrl();
        updateUserPaths(apiUrl);
        setOpen(false);
    }

    const getApiUrl = () => {
        if (type === 'loadFolder') {
            return ApiHelper.getApiUrl(DvtApiPaths.ChangePath.UpdateUserLoadFolderPaths);
        } else if (type === 'logFolder') {
            return ApiHelper.getApiUrl(DvtApiPaths.ChangePath.UpdateUserLogFolderPaths);
        } else if (type === 'productionFolder') {
            return ApiHelper.getApiUrl(DvtApiPaths.ChangePath.UpdateUserProductionFolderPaths);
        }
    }

    const handleSetDefaultPath = (path) => {
        if (type === 'loadFolder') {
            setDefaultLoadPath(path);
        } else if (type === 'logFolder') {
            setDefaultLogPath(path);
        } else if (type === 'productionFolder') {
            setDefaultProdPath(path);
        }
    }

    const updateUserPaths = async (apiUrl) => {
        try {
            setLoader(true);
            const response = await axios.post(apiUrl, {
                userInfoId: userId,
                [type]: selectedPath[selectedPath.length - 1],
                updateBy: userEmail
            });
            setLoader(false);
            handleSetDefaultPath(selectedPath);
        } catch (error) {
            setLoader(false);
            console.error('Error updating user paths:', error);
        }
    }

    return (
        <div className="change-paths-container">
            <h4 className={style.changePathsTitle}>Change Paths</h4>
            <hr />
            <div className={style.table}>
                <div className={style.row}>
                    <div className={`${style.itemTitle} ${style.item}`}>Load Folder</div>
                    <div className={`${style.folderIcon} ${style.item}`} onClick={
                        () => {
                            setType('loadFolder');
                            setOpen(true);
                            setSelectedPath(defaultLoadPath);
                        }
                    }>
                        <img src="../asset/folder-icon.svg" alt="Folder Icon" className={style.icon} />
                    </div>
                    <div className={`${style.paths} ${style.item}`}>{transformPath(defaultLoadPath)}</div>
                </div>
                <div className={style.row}>
                    <div className={`${style.itemTitle} ${style.item}`}>Log Folder</div>
                    <div className={`${style.folderIcon} ${style.item}`} onClick={
                        () => {
                            setType('logFolder');
                            setOpen(true);
                            setSelectedPath(defaultLogPath);
                        }
                    }>
                        <img src="../asset/folder-icon.svg" alt="Folder Icon" className={style.icon} />
                    </div>
                    <div className={`${style.paths} ${style.item}`}>{transformPath(defaultLogPath)}</div>
                </div>
                <div className={style.row}>
                    <div className={`${style.itemTitle} ${style.item}`}>Production Folder</div>
                    <div className={`${style.folderIcon} ${style.item}`} onClick={
                        () => {
                            setType('productionFolder');
                            setOpen(true);
                            setSelectedPath(defaultProdPath);
                        }
                    }>
                        <img src="../asset/folder-icon.svg" alt="Folder Icon" className={style.icon} />
                    </div>
                    <div className={`${style.paths} ${style.item}`}>{transformPath(defaultProdPath)}</div>
                </div>
            </div>
            <div className={`${style.buttonGroup} ${style.row}`}>
                <CustomButton
                    variant="contained"
                    color="primary"
                    onClick={() => {
                        history.push('/home');
                    }}
                >
                    Cancel
                </CustomButton>
            </div>
            <React.Suspense fallback={<div>Loading...</div>}>
                <CustomDialog
                    className="changePathsDialog"
                    title="Choose a folder"
                    content={
                        treeData.length > 0 ? (
                            <ChangePathsTree
                                treeData={treeData}
                                defaultPath={selectedPath}
                                pathChange={pathChange} />
                        ) : (
                            <div>No folders available</div>
                        )
                    }
                    open={open}
                    customWidth={330}
                    onClose={() => console.log('Dialog closed')}
                    actions={[
                        {
                            label: 'Update Path',
                            disabled: selectedPath.length === 0,
                            title: selectedPath.length === 0 ? "Please select a folder" : "",
                            onClick: () => {
                                setOpen(false);
                                handlePathChange();

                            },
                        },
                        {
                            label: 'Close',
                            onClick: () => {
                                setOpen(false);
                            },
                        },
                    ]}
                />
            </React.Suspense>
            {loader && (
                <React.Suspense fallback={null}>
                    <FullPageLoader />
                </React.Suspense>
            )}
        </div>
    );
}

export default ChangePaths;