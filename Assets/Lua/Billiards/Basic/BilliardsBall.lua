local BilliardsBall = class("BilliardsBall")

local SLEEP_VALUE = 0.1

function BilliardsBall:Awake()
    self:Init(self.component.gameObject)
end

function BilliardsBall:Init(go)
    self.gameObject = go
    self.transform = go.transform
    self.ball = self.transform:Find("ball")
    self.rigidBody = self.ball:GetComponent(typeof(Rigidbody))
    self.circle = self.transform:Find("circle")
    self.circle:SetActive(false)

    self.trailEffect = self.transform:Find("trailEffect")
    if self.trailEffect then
        self.trailEffect:SetActive(false)
    end

    self.anim_Rotate = self.circle.gameObject:GetComponent(typeof(Animator))

    self:InitData()
end

function BilliardsBall:InitData()
    -- 球的编号
    self.num = 0
    self.type = 0
    self.live = false

    self.posX = 0
    self.posY = 0
    self.vx = 0
    self.vy = 0
    self.angle = 0
    self._dirtyPos = true
end

function BilliardsBall:SetLive(live)
    self.live = live
end

function BilliardsBall:IsLive()
    return self.live
end

function BilliardsBall:SetCircleActive(active)
    self.circle:SetActive(active)
    if active then
        self.anim_Rotate:Play("rotate")
    end
end

-- 球的数字
function BilliardsBall:SetNum(num)
    self.num = num
end
function BilliardsBall:GetNum()
    return self.num
end

-- 球的类型
function BilliardsBall:SetType(type)
    self.type = type
end
function BilliardsBall:GetType()
    return self.type
end

-- 球的位置
function BilliardsBall:SetPosition(x, y)
    if x and y then
        self.posX = x
        self.posY = y
        self._dirtyPos = true
    end
end

function BilliardsBall:GetPosition()
    return self.posX, self.posY
end

function BilliardsBall:GetRotation()
    return self.r_x, self.r_y, self.r_z
end

function BilliardsBall:SetRotation(x, y, z)
    if x and y and z then
        self.r_x = x or 0
        self.r_y = y or 0
        self.r_z = z or 0
        self.ball.transform.localRotation = Quaternion.Euler(self.r_x, self.r_y, self.r_z)
    end
end

function BilliardsBall:IsStopped()
    return self.vx == 0 and self.vy == 0
end

function BilliardsBall:IsStopping()
    return math.abs(self.vx) <= SLEEP_VALUE and math.abs(self.vy) <= SLEEP_VALUE
end

function BilliardsBall:SetVel(vx, vy)
    if vx and vy then
        self.vx = vx
        self.vy = vy
        self._dirtyAngle = true
    end
end

function BilliardsBall:GetVel()
    return self.vx or 0, self.vy or 0
end

function BilliardsBall:ChangeAngle(angle)
    self.angle = angle
    self._dirtyAngle = true
end

function BilliardsBall:SetData(x, y, vx, vy)
    self:SetVel(vx, vy)
    self:SetPosition(x, y)
end

function BilliardsBall:SetPowerData(power, x, y, to_x, to_y)
    print("SetPowerData++++++++++++++", to_x, to_y, x, y, power, self.posX, self.posY)
    self:SetPosition(math.fixed3(x), math.fixed3(y))
    power = math.fixed3(power)
    local dx = math.fixed3(to_x) - self.posX
    local dy = math.fixed3(to_y) - self.posY
    local spring = math.fixed3(math.sqrt(dx * dx + dy * dy))
    self:SetVel(math.fixed3(math.fixed3(dx / spring) * power), math.fixed3(math.fixed3(dy / spring) * power))
    print("SetPowerData---------------", self.vx, self.vy)
end

function BilliardsBall:UpdateState()
    if self._dirtyPos then
        self._dirtyPos = false
        self.transform:SetLocalPosition(self.posX, 0, self.posY)
    end

    if self._dirtyAngle then
        self._dirtyAngle = false
        local rotationAxis = Vector3.Cross(Vector3.up, Vector3(self.vx, 0, self.vy)).normalized
        self.ball.localRotation = Quaternion.Euler(rotationAxis * self.angle) * self.ball.transform.localRotation

        if self.trailEffect then
            self.trailEffect:SetActive(not self:IsStopped())
        end
    end
end

-- function BilliardsBall:IsInHole()
--     return BillardsInfo.GetBallById(self.ball_num) == nil
-- end

return BilliardsBall
