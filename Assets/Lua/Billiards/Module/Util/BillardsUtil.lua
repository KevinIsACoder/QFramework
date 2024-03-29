---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by lengyouyi.
--- DateTime: 2022/3/21 20:48
---
--- 工具类
local BillardsUtil = {}
local self = BillardsUtil

function BillardsUtil.getBallSprite(name)
    local ballAtlas = Spark.Assets.LoadAsset("Game/Billiards/Atlas/BillardsBallTexture", typeof(Spark.UIAtlas))
    return ballAtlas:GetSprite(name)
end

--- 世界坐标转UI坐标
function BillardsUtil.ConvertWorldtoUI(worldPos, rectTransform)
    -- 世界坐标转屏幕坐标
    local vec3 = self.ConvertWorldtoScreen(worldPos)
    local screenPos = Vector2(vec3.x, vec3.y)
    -- 屏幕坐标转UI坐标
    return self.ConvertScreentoUI(screenPos, rectTransform)
end

--- UI世界坐标转场景世界坐标
--- position UI的世界坐标
function BillardsUtil.ConvertUIWorldtoSceneWorld(position)
    -- UI坐标转屏幕坐标
    local vec3 = self.ConvertUIWorldtoScreen(position)
    local screenPos = Vector2(vec3.x, vec3.y)
    -- 屏幕坐标转世界坐标
    return self.ConvertScreentoWorld(screenPos)
end

--- 屏幕坐标转世界坐标
function BillardsUtil.ConvertScreentoWorld(screenPos)
    local camera3D = BilliardsTable.camera3D
    local vec3 = Vector3(screenPos.x, screenPos.y, 0)
    local worldPos = camera3D:ScreenToWorldPoint(vec3)
    return worldPos
end

--- 场景世界坐标转屏幕坐标
function BillardsUtil.ConvertWorldtoScreen(worldPos)
    local camera3D = BilliardsTable.camera3D
    return camera3D:WorldToScreenPoint(worldPos)
end

--- UI世界坐标转屏幕坐标
function BillardsUtil.ConvertUIWorldtoScreen(uiWorldPos)
    local cameraUI = BilliardsTable.cameraUI
    return cameraUI:WorldToScreenPoint(uiWorldPos)
end

--- 屏幕坐标转UI坐标
function BillardsUtil.ConvertScreentoUI(screenPos, rectTransform)
    if not rectTransform then return end
    local cameraUI = BilliardsTable.cameraUI
    local isIn, UIPosition = RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPos, cameraUI, nil)
    return UIPosition
end

function BillardsUtil.GetLineLength(from, to)
    return self.Vector2Distance(from.x, from.y, to.x, to.y)
end

function BillardsUtil.Vector3Cross(x1, y1, z1, x2, y2, z2)
    local x = y1 * z2 - z1 * y2
    local y = z1 * x2 - x1 * z2
    local z = x1 * y2 - y1 * x2
    return {x = x, y = y, z = z}
end

function BillardsUtil.Vector2Distance(x1, y1, x2, y2)
    local x = x1 - x2
    local y = y1 - y2
    return math.sqrt(x * x + y * y)
end

function BillardsUtil.Vector2Dot(x1, y1, x2, y2)
    return x1 * x2 + y1 * y2
end

function BillardsUtil.Vector2Normalize(x, y)
    local magnitude = math.sqrt(x * x + y * y)
	if magnitude > 1e-05 then
        return x / magnitude, y / magnitude
    end
    return 0, 0
end

function BillardsUtil.Vector3Normalize(x, y, z)
    local magnitude = math.sqrt(x * x + y * y + z * z)
	if magnitude > 1e-05 then
        return x / magnitude, y / magnitude, z / magnitude
    end
    return 0, 0, 0
end

-- Pack / Unpack
function BillardsUtil.PackVec3(posX, posY)
    return Vector3(posX, 0, posY)
end
function BillardsUtil.UnpackVec3(vec)
    return vec.x, vec.z
end

function BillardsUtil.PackVec2(posX, posY)
    return Vector2(posX, posY)
end
function BillardsUtil.UnpackVec2(vec)
    return vec.x, vec.y
end

return BillardsUtil