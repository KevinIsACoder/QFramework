local UIBilliardsResult = class("UIBilliardsResult", BaseUIBilliardsResult)
local EventDef = BillardsEvent.BillardsEventType
function UIBilliardsResult:OnOpened()
    self.resultData = self.context:Get("resultdata")
    table.sort(self.resultData, function(a, b)
        return a.result < b.result
    end)
    
    local trans = self.view.rankContent.transform
    local obj = self.view.rankItem.gameObject
    UIHelper.CreatePart(obj, trans, #self.resultData, function(index, part)
        local fstImg = part:Get("fstImg")
        local secImg = part:Get("secImg")
        local playerName = part:Get("playerName")
        local point = part:Get("point")
        local cueNum = part:Get("cueNum")
        local head = part:Get("Head")
        local btnInvite = part:Get("btnInvite")
        btnInvite.onClick:RemoveAllListeners()
        btnInvite.onClick:AddListener(function()
            self:FollowFunc(index)
        end)
        local data = self.resultData[index]
        local isMetal = data.result == 1
        fstImg:SetActive(isMetal)
        secImg:SetActive(not isMetal)

        point.text = data.packet_num
        cueNum.text = data.strokes

        if data.m_headUrl and data.m_headUrl ~= " " then
            local headCom = head.transform:GetChild(0)
            UIHelper.SetHead(headCom, data.m_headUrl, self:GetLoader())
        end

        local alignment = I18nManager.IsRTL() and UnityEngine.TextAnchor.MiddleRight or UnityEngine.TextAnchor.MiddleLeft
        UIHelper.SetTextAlignment(playerName, alignment)
        playerName.text = I18nManager.ApplyRTLfix(data.m_nick)
        if not self.inviteBtn then self.inviteBtn = {} end
        self.inviteBtn[index] = btnInvite
    end)

    local isWin = self:IsWinner()
    self.view.winGroup:SetActive(isWin)
    
    local title = ""
    if self:IsViewer() then
        title = I18nManager.GetCurrentValue("Billiards_SettleMent", true)
    else
        title = isWin and I18nManager.GetCurrentValue("Billiards_Win", true) or I18nManager.GetCurrentValue("Billiards_Lose", true)
    end
    self.view.lblTitle.text = title

    local userIds = {}
    for i = 1, #self.resultData do
        local resultData = self.resultData[i]
        if self:IsViewer() then
            self.inviteBtn[i].gameObject:SetActive(false)
        else
            table.insert(userIds, resultData.m_userId)
        end
    end

    local cb = function(jsonData)
        if jsonData then
            local follow_info = jsonData.follow_info
            if follow_info then
                self:ShowFollowBtn(follow_info, userIds)
            end
        end
    end
    
    local body = {user_ids = userIds}
    HttpManager.Post(NetCfg.http_follow, body, function(info)
        info = json.decode(info)
        cb(info)
    end)

    self.view.btnAgain:SetActive(not self:IsViewer())
end

function UIBilliardsResult:IsViewer()
    for _, data in pairs(self.resultData) do
        if data.m_userId == UserInfo.GetUserId() then
            return false
        end
    end
    return true
end

function UIBilliardsResult:IsWinner()
    for _, v in pairs(self.resultData) do
        if v.m_userId == UserInfo.GetUserId() then
            return v.result == 1
        end
    end
end

function UIBilliardsResult:FollowFunc(index)
    local data = self.resultData[index]
    print("   去关注   ", data.m_userId)
    local selfId = UserInfo.GetUserId() --自己的id
    local jk = string.gsub(NetCfg.http_to_follow, "userid", selfId, 1)
    local body = {data.m_userId .. ""}
    local cb = function(data)
        if data then
            VoiceSender.FollowUserSuccess(data.m_userId .. "")
        end
    end
    
    HttpManager.Post(jk, body, function(info)
        local data = nil
        if info.text then
            data = json.decode(info.text)
        end
        cb(data)
    end)

    self.inviteBtn[index]:SetActive(false)
end

function UIBilliardsResult:OnButtonClick(com)
    if com == self.view.btnClose then
        self:Exit()
        VoiceSender.ContinueGame(0, 0)
    elseif com == self.view.btnAgain then
        self:Exit()
        VoiceSender.ContinueGame(999, 1)
    end
end

function UIBilliardsResult:Exit()
    self:Close()
    EventManager.Dispatch(EventDef.E_ExitGame)
end

--- 是否显示关注按钮
function UIBilliardsResult:ShowFollowBtn(data, userIds)
    if data then
        for i = 1, #userIds do
            local userId = userIds[i]
            if userId == UserInfo.GetUserId() then
                self.inviteBtn[i].gameObject:SetActive(false)
            else
                local followStatus = data[tostring(userId)]
                if followStatus == 0 or followStatus == 2 then
                    self.inviteBtn[i].gameObject:SetActive(true)
                else
                    self.inviteBtn[i].gameObject:SetActive(false)
                end
            end
        end
    end
end

return UIBilliardsResult