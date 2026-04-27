import moment from 'moment';
/* This file provides some public methods for the AppIventory project */

/* 
    Deep copy an object or array
    @param {Object | Array}  obj
    @return {Object | Array} A deeply copied object or array
*/
const deepClone = (obj) => {
    let newObj = Array.isArray(obj) ? [] : {}
    if (obj && typeof obj === "object") {
        for (let key in obj) {
            if (obj.hasOwnProperty(key)) {
                newObj[key] = (obj && typeof obj[key] === 'object' && obj[key] !== null) ? deepClone(obj[key]) : obj[key];
            }
        }
    }
    return newObj
}

/* 
    Array removes duplicate data based on key
    @param {Array}  array
    @param {String}  key
    @return {Array} An array with duplicate data removed
*/
const unique = (array, key) => {
    const res = new Map()
    return array.filter(item => !res.has(item[key]) && res.set(item[key], 1))
}

/* 
    Flatten the array
    @param {Array}  arr   The array you want to flatten
    @return {Array} result The flattened array
*/
const flatten = (arr) => {
    var result = [];
    for (var i = 0, len = arr.length; i < len; i++) {
        if (Array.isArray(arr[i])) {
            result = result.concat(flatten(arr[i]))
        }
        else {
            result.push(arr[i])
        }
    }
    return result;
}

/* 
  A method to convert a file format to Base64 format
*/
const getBase64 = file => {
    return new Promise(resolve => {
        let baseURL = "";
        let reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = () => {
            baseURL = reader.result;
            resolve(baseURL);
        };
    });
};

const covertBlobTobase64 = (blob) => {
    return new Promise(resolve => {
        const reader = new FileReader();
        reader.readAsDataURL(blob);
        reader.onload = (e) => {
            const base64 = e.target.result
            resolve(base64)
        }
    });
}


/* 
  fileType  text/plain or text/csv
*/
const download = (filename, fileType, text) => {
    let element = document.createElement('a')
    let blob = new Blob(["\ufeff" + text], { type: `${fileType};charset=utf-8;` });
    let url = URL.createObjectURL(blob);

    element.href = url;
    element.setAttribute('download', filename);
    document.body.appendChild(element)
    element.click();
    document.body.removeChild(element);
}

/* 
 Array objects, grouped by a field
*/
const groupedByField = (arr, field) => {
    let map = {}
    for (let i = 0; i < arr.length; i++) {
        let currentItem = arr[i]
        if (!map[currentItem[field]]) {
            map[currentItem[field]] = [currentItem]
        } else {
            map[currentItem[field]].push(currentItem)
        }
    }
    let result = []
    Object.keys(map).forEach(key => {
        result.push({
            groupName: key,
            data: map[key],
        })
    })
    return result
}


const removeObjectSpace = (targetObject) => {
    for (let [key, value] of Object.entries(targetObject)) {
        if (typeof (value) === "string") {
            targetObject[key] = targetObject[key].trim()
        }
    }
    return targetObject
}


/* 
    Check whether two objects are equal
    @param {Object} Object1 
    @param {Object} Object2
    return true euqal  false not equal
*/
const isObjectEqual = (object1, object2) => {
    let props1 = Object.getOwnPropertyNames(object1)
    let props2 = Object.getOwnPropertyNames(object2)
    if (props1.length !== props2.length) {
        return false
    }
    for (let i = 0; i < props1.length; i++) {
        let propName = props1[i]
        if (object1[propName] !== object2[propName]) {
            return false
        }
    }

    return true
}

/* 
    Copy the text to the clipboard
*/

const copyTextToClipboard = async (copyContent, callback) => {
    try {
        await navigator.clipboard.writeText(copyContent);
        /* Resolved - text copied to clipboard successfully */
        callback('success')
    } catch (err) {
        /* Rejected - text failed to copy to the clipboard */
        callback('error')
    }
}

const frontMove = (arr, index) => {
    if (index < 1) return arr
    arr[index] = arr.splice(index - 1, 1, arr[index])[0]
    return arr
}

const backMove = (arr, index) => {
    if (index >= arr.length - 1) return arr
    arr[index] = arr.splice(index + 1, 1, arr[index])[0]
    return arr
}

const checkRepeat = (arr, key) => {
    let object = {}
    let repeatList = []
    arr.forEach((item) => {
        if (object[item[key]]) {
            repeatList.push(item[key])
        } else {
            object[item[key]] = item[key]
        }
    });
    return [...new Set(repeatList)]
}

const getTimestampInSeconds = () => {
    return Math.floor(Date.now() / 1000)
}

const convertUTCTimeToLocalTime = (utcDate, format = "MM/DD/YYYY hh:mm:ss A") => {
    return moment.utc(utcDate).local().format(format);
}


export {
    deepClone,
    unique,
    flatten,
    getBase64,
    covertBlobTobase64,
    download,
    groupedByField,
    removeObjectSpace,
    isObjectEqual,
    copyTextToClipboard,
    frontMove,
    backMove,
    checkRepeat,
    getTimestampInSeconds,
    convertUTCTimeToLocalTime
}