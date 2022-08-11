--collectgarbage("setpause", 99)
local _G = _G
local rawset = rawset

-- 屏蔽xlua的CS
local _CS = CS
_G["CS"] = nil

function declare(name, value)
    if string.match(name, "^__%u[%u%d_]*$") then
        error("attempt to declare variable: " .. name, 2)
        return
    end
    __spark_reload_global[name] = true
    rawset(_G, name, value)
end

function undeclare(name, path)
    rawset(_G, name, nil)
    package.loaded[path] = nil
    __spark_reload_global[name] = false
end

function declared(name)
    return rawget(_G, name) ~= nil
end

local __using_global = {}
function using(name, path)
    if not path then
        path = name
        name = string.sub(path, string.find(path, "[^\\.]*$"))
    end
    __using_global[name] = path
end
function unusing(name)
    local path = __using_global[name]
    if path then
        if __spark_reload_global[name] then
            undeclare(name, path)
        end
        __using_global[name] = nil
    end
end

local __imported_packages = {}
function import(...)
    local fns = _CS.__fns
    local len = #__imported_packages
    for _, package in ipairs({...}) do
        if fns[package] then
            len = len + 1
            __imported_packages[len] = package
        end
    end
end

setmetatable(_G, {
    __index = function(_, key)
        if string.match(key, "^__%u[%u%d_]*$") then
            return false
        end
        local value = nil
        local path = __using_global[key]
        if path then
            value = require(path)
            __spark_reload_global[key] = true
        else
            local fns = _CS.__fns
            for _, package in ipairs(__imported_packages) do
                local name = package .. "." .. key
                if fns[name] then
                    if not __imported_packages[package] then
                        if string.find(package, "%.") then
                            value = _CS
                            local ns = string.split(package, "%.")
                            for _, v in ipairs(ns) do
                                value = value[v]
                            end
                        else
                            value = _CS[package]
                        end
                        __imported_packages[package] = value
                    end
                    value = __imported_packages[package][key]
                    break
                end
            end
            if not value and fns[key] then value = _CS[key] end
        end
        if value then
            rawset(_G, key, value)
            return value
        end
        error("attempt to read undeclared variable: " .. key, 2)
    end,
    __newindex = function(_, key)
        error("attempt to write undeclared variable: " .. key, 2)
    end
})

-- 导入部分类库
import("UnityEngine", "UnityEngine.UI", "Spark")

-- @module Lua
-- declare("class", require("Spark.Lua.middleclass"))